using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace backend.Services;

public class SpotifyOptions
{
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";

    // How many Spotify API requests the enricher may have in flight at once.
    // Bounded by the HttpClient's MaxConnectionsPerServer (see Program.cs). 1 = sequential.
    public int MaxConcurrency { get; set; } = 5;
}

public class SpotifyEnricher
{
    private const int SpotifyPageSize = 50; // /tracks and /artists both cap at 50 ids per call
    private const int MaxRetries = 6;

    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly SpotifyOptions _opt;
    private readonly ILogger<SpotifyEnricher> _log;

    // Cached client-credentials token (refreshed on expiry or a 401).
    private string? _token;
    private DateTimeOffset _tokenExpiresUtc = DateTimeOffset.MinValue;

    // Serialises token refreshes when several parallel requests hit a 401 at once.
    private readonly SemaphoreSlim _tokenLock = new(1, 1);

    // Bounds how many Spotify requests are in flight at once; created per enrichment run.
    private SemaphoreSlim? _httpGate;

    public SpotifyEnricher(
        HttpClient http,
        AppDbContext db,
        IOptions<SpotifyOptions> opt,
        ILogger<SpotifyEnricher> log)
    {
        _http = http;
        _db = db;
        _opt = opt.Value;
        _log = log;

        string id = _opt.ClientId?.Trim() ?? "";
        string idPreview = string.IsNullOrEmpty(id) ? "<empty>" : id[..Math.Min(6, id.Length)];
        _log.LogInformation(
            "SpotifyOptions loaded. ClientId starts with: {Prefix}. ClientSecret set: {HasSecret}",
            idPreview, !string.IsNullOrWhiteSpace(_opt.ClientSecret));
    }

    private static string? TrackIdFromUri(string? uri)
        => string.IsNullOrWhiteSpace(uri) ? null : (uri.Split(':') is { Length: 3 } p ? p[2] : null);

    // -------------------- PUBLIC API (signatures unchanged) --------------------
    public Task<int> BackfillAllAsync(CancellationToken ct = default) => RunBackfillAsync("full", ct);
    public Task<int> BackfillDeltaAsync(CancellationToken ct = default) => RunBackfillAsync("delta", ct);

    // Both backfills do the same work — tag rows with TrackId, then enrich any
    // track ids not yet in the catalog. ("delta" is implicit: we only ever fetch
    // ids that are missing from TracksCatalog.)
    private async Task<int> RunBackfillAsync(string mode, CancellationToken ct)
    {
        using IDisposable? scope = _log.BeginScope("job:{JobId}", Guid.NewGuid().ToString("N"));
        _log.LogInformation("{Mode} backfill started", mode);

        await BackfillTrackIdsOnSpotifyAsync(ct);

        // MySQL-friendly NOT EXISTS (instead of Except)
        List<string> unknownTrackIds = await _db.Spotify
            .Where(s => s.TrackId != null)
            .Where(s => !_db.TracksCatalog.Any(t => t.TrackId == s.TrackId))
            .Select(s => s.TrackId!)
            .Distinct()
            .ToListAsync(ct);

        _log.LogInformation("Found {Count} unknown track IDs", unknownTrackIds.Count);

        int inserted = await EnrichTracksAsync(unknownTrackIds, ct);
        _log.LogInformation("{Mode} backfill finished. Inserted {Inserted} tracks", mode, inserted);
        return inserted;
    }

    // Fill the TrackId column for rows that have a uri but no parsed id yet.
    private async Task BackfillTrackIdsOnSpotifyAsync(CancellationToken ct)
    {
        List<Spotify> rows = await _db.Spotify
            .Where(s => s.spotify_track_uri != null && s.TrackId == null)
            .ToListAsync(ct);

        foreach (Spotify r in rows) r.TrackId = TrackIdFromUri(r.spotify_track_uri);

        if (rows.Count > 0)
        {
            await _db.SaveChangesAsync(ct);
            _log.LogInformation("Filled TrackId for {Count} rows", rows.Count);
        }
        else
        {
            _log.LogInformation("No rows needed TrackId backfill");
        }
    }

    private sealed record ParsedTrack(
        string Id,
        string? Name,
        string? AlbumId,
        string? AlbumName,
        List<string> ArtistIds);

    // -------------------- core enrichment --------------------
    private async Task<int> EnrichTracksAsync(List<string> trackIds, CancellationToken ct)
    {
        int total = trackIds.Count;
        if (total == 0)
        {
            _log.LogInformation("No work to do");
            return 0;
        }

        int concurrency = Math.Max(1, _opt.MaxConcurrency);
        _httpGate = new SemaphoreSlim(concurrency, concurrency); // bound in-flight Spotify requests
        _log.LogInformation(
            "Enriching {Total} tracks in batches of {Size}, up to {Concurrency} requests in parallel...",
            total, SpotifyPageSize, concurrency);
        await EnsureTokenAsync(ct);

        // Oracle's MySql.EntityFrameworkCore can't translate a parameterized collection
        // (`list.Contains(col)`) — it fails to type-map the parameter, and neither
        // TranslateParameterizedCollectionsToConstants() nor EF.Constant() overrides it.
        // So load the existing keys once and track membership in memory. These sets are
        // only ever read/mutated on this single owning thread — the parallelism below is
        // confined to the HTTP fetches and never touches the DB or these collections.
        HashSet<string> knownTracks =
            (await _db.TracksCatalog.Select(x => x.TrackId).ToListAsync(ct)).ToHashSet();
        HashSet<string> knownArtists =
            (await _db.ArtistsCatalog.Select(x => x.ArtistId).ToListAsync(ct)).ToHashSet();
        HashSet<string> knownLinks =
            (await _db.TrackArtists.Select(x => new { x.TrackId, x.ArtistId }).ToListAsync(ct))
            .Select(x => x.TrackId + "|" + x.ArtistId).ToHashSet();
        Dictionary<string, List<string>> genresByArtist =
            (await _db.ArtistGenres.Select(g => new { g.ArtistId, g.Genre }).ToListAsync(ct))
            .GroupBy(x => x.ArtistId)
            .ToDictionary(grp => grp.Key, grp => grp.Select(x => x.Genre).ToList());

        int inserted = 0, processed = 0, waveNo = 0;
        string[][] batches = trackIds.Chunk(SpotifyPageSize).ToArray();
        int totalWaves = (int)Math.Ceiling(batches.Length / (double)concurrency);

        // Work in waves of `concurrency` batches: fetch the wave's tracks (and the artists
        // they reference) from Spotify in parallel, then write the wave to the DB here on
        // the single owning thread (EF's DbContext is not thread-safe). Committing per wave
        // keeps the job resumable — a cancelled run resumes from the last committed wave and
        // only ever fetches ids still missing from the catalog (the incremental behaviour).
        foreach (string[][] wave in batches.Chunk(concurrency))
        {
            waveNo++;

            // 1) fetch + parse all track batches in this wave concurrently
            List<ParsedTrack> parsed =
                (await Task.WhenAll(wave.Select(b => FetchParsedTracksAsync(b, ct))))
                .SelectMany(p => p)
                .ToList();

            // 2) insert the tracks we don't already have (membership checked in memory)
            List<ParsedTrack> newParsed = parsed.Where(p => !knownTracks.Contains(p.Id)).ToList();
            if (newParsed.Count > 0)
            {
                DateTime now = DateTime.UtcNow;
                _db.TracksCatalog.AddRange(newParsed.Select(p => new TrackCatalog
                {
                    TrackId = p.Id, Name = p.Name, AlbumId = p.AlbumId, AlbumName = p.AlbumName, FetchedAtUtc = now
                }));
                await _db.SaveChangesAsync(ct);
                foreach (ParsedTrack p in newParsed) knownTracks.Add(p.Id);
                inserted += newParsed.Count;
            }

            // 3) ensure all referenced artists exist — fetch the unknown ones in parallel
            List<string> unknownArtists = parsed
                .SelectMany(p => p.ArtistIds)
                .Distinct()
                .Where(a => !knownArtists.Contains(a))
                .ToList();

            if (unknownArtists.Count > 0)
            {
                List<JsonElement> rawArtists =
                    (await Task.WhenAll(unknownArtists.Chunk(SpotifyPageSize)
                        .Select(aBatch => GetWithRetryAsync(() => GetArtistsAsync(aBatch, ct), ct))))
                    .SelectMany(a => a)
                    .ToList();

                DateTime now = DateTime.UtcNow;
                List<ArtistCatalog> newArtists = new();
                List<ArtistGenre> newGenres = new();

                foreach (JsonElement a in rawArtists)
                {
                    if (a.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) continue;

                    string artistId = a.GetProperty("id").GetString()!;
                    if (!knownArtists.Add(artistId)) continue; // already have it (or a dupe within the wave)

                    string? name = a.GetProperty("name").GetString();
                    newArtists.Add(new ArtistCatalog { ArtistId = artistId, Name = name, FetchedAtUtc = now });

                    if (a.TryGetProperty("genres", out JsonElement g) && g.ValueKind == JsonValueKind.Array)
                    {
                        if (!genresByArtist.TryGetValue(artistId, out List<string>? genres))
                            genres = genresByArtist[artistId] = new List<string>();

                        foreach (JsonElement node in g.EnumerateArray())
                        {
                            string? genre = node.GetString();
                            if (!string.IsNullOrWhiteSpace(genre) &&
                                !genres.Contains(genre)) // guards the (ArtistId, Genre) PK against dupes
                            {
                                genres.Add(genre);
                                newGenres.Add(new ArtistGenre { ArtistId = artistId, Genre = genre });
                            }
                        }
                    }
                }

                if (newArtists.Count > 0) _db.ArtistsCatalog.AddRange(newArtists);
                if (newGenres.Count > 0) _db.ArtistGenres.AddRange(newGenres);
                if (newArtists.Count > 0 || newGenres.Count > 0) await _db.SaveChangesAsync(ct);
            }

            // 4) link tracks <-> artists; skip ids Spotify didn't return so the FK can't break
            List<TrackArtist> newLinks = new();
            foreach (ParsedTrack p in parsed)
            foreach (string artistId in p.ArtistIds)
                if (knownArtists.Contains(artistId) && knownLinks.Add(p.Id + "|" + artistId))
                    newLinks.Add(new TrackArtist { TrackId = p.Id, ArtistId = artistId });

            if (newLinks.Count > 0)
            {
                _db.TrackArtists.AddRange(newLinks);
                await _db.SaveChangesAsync(ct);
            }

            // 5) genre weights for the just-inserted tracks (uses the in-memory genre map)
            if (newParsed.Count > 0) await BuildWeightsAsync(newParsed, genresByArtist, ct);

            processed += wave.Sum(b => b.Length);
            _log.LogInformation("Wave {Wave}/{TotalWaves} — progress {Processed}/{Total} ({Percent:P1})",
                waveNo, totalWaves, processed, total, processed / (double)total);

            _db.ChangeTracker.Clear(); // keep change tracking cheap across a long job
        }

        _log.LogInformation("Enrichment complete. Total inserted tracks: {Inserted}", inserted);
        return inserted;
    }

    // Fetch + parse one batch of track ids into our small shape. Pure HTTP/JSON work with
    // no shared state, so several of these run safely in parallel.
    private async Task<List<ParsedTrack>> FetchParsedTracksAsync(string[] batch, CancellationToken ct)
    {
        List<JsonElement> rawTracks = await GetWithRetryAsync(() => GetTracksAsync(batch, ct), ct);

        List<ParsedTrack> parsed = new();
        foreach (JsonElement t in rawTracks)
        {
            if (t.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) continue;

            string id = t.GetProperty("id").GetString()!;
            string? name = t.GetProperty("name").GetString();
            JsonElement album = t.GetProperty("album");
            string? albumId = album.TryGetProperty("id", out JsonElement aid) ? aid.GetString() : null;
            string? albumName = album.TryGetProperty("name", out JsonElement an) ? an.GetString() : null;

            List<string> artistIds = new();
            foreach (JsonElement a in t.GetProperty("artists").EnumerateArray())
                artistIds.Add(a.GetProperty("id").GetString()!);

            parsed.Add(new ParsedTrack(id, name, albumId, albumName, artistIds));
        }

        return parsed;
    }

    // Genre weights for a set of tracks, computed from the caller's in-memory genre
    // map (the provider can't translate a parameterized .Contains, see EnrichTracksAsync).
    private async Task BuildWeightsAsync(
        List<ParsedTrack> tracks, Dictionary<string, List<string>> genresByArtist, CancellationToken ct)
    {
        DateTime now = DateTime.UtcNow;
        List<TrackGenreWeight> rows = new();

        foreach (ParsedTrack t in tracks)
        {
            if (t.ArtistIds.Count == 0) continue;
            int aCount = t.ArtistIds.Count;
            Dictionary<string, double> weights = new();

            foreach (string a in t.ArtistIds)
            {
                if (!genresByArtist.TryGetValue(a, out List<string>? genres) || genres.Count == 0) continue;
                double share = 1.0 / aCount / genres.Count;
                foreach (string g in genres)
                    weights[g] = (weights.TryGetValue(g, out double w) ? w : 0) + share;
            }

            rows.AddRange(weights.Select(kv => new TrackGenreWeight
            {
                TrackId = t.Id, Genre = kv.Key, Weight = kv.Value, BuiltAtUtc = now
            }));
        }

        if (rows.Count > 0)
        {
            _db.TrackGenreWeights.AddRange(rows);
            await _db.SaveChangesAsync(ct);
        }
    }

    // ---------------- token: cached + refreshed ----------------
    // staleToken != null means "refresh because this token just 401'd". The lock + the
    // staleToken check ensure that when several parallel requests 401 at once, only one
    // refresh happens — the rest see the already-replaced token and return.
    private async Task EnsureTokenAsync(CancellationToken ct, string? staleToken = null)
    {
        bool forced = staleToken != null;
        if (!forced && _token != null && DateTimeOffset.UtcNow < _tokenExpiresUtc)
            return;

        await _tokenLock.WaitAsync(ct);
        try
        {
            // Re-check under the lock: a parallel caller may have refreshed while we waited.
            if (!forced && _token != null && DateTimeOffset.UtcNow < _tokenExpiresUtc)
                return;
            if (forced && _token != staleToken)
                return; // someone already replaced the token that 401'd

            string id = _opt.ClientId?.Trim() ?? "";
            string secret = _opt.ClientSecret?.Trim() ?? "";
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Spotify ClientId/ClientSecret are not configured.");

            using HttpRequestMessage req = new(HttpMethod.Post, "https://accounts.spotify.com/api/token");
            string basic = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{id}:{secret}"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials"
            });

            using HttpResponseMessage res = await _http.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                string body = await res.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException(
                    $"Token request failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {body}");
            }

            await using Stream s = await res.Content.ReadAsStreamAsync(ct);
            using JsonDocument json = await JsonDocument.ParseAsync(s, cancellationToken: ct);
            JsonElement root = json.RootElement;

            _token = root.GetProperty("access_token").GetString()!;
            int expiresIn = root.TryGetProperty("expires_in", out JsonElement e) ? e.GetInt32() : 3600;
            _tokenExpiresUtc = DateTimeOffset.UtcNow.AddSeconds(expiresIn - 60); // refresh a minute early
        }
        finally
        {
            _tokenLock.Release();
        }
    }

    // ---------------- Spotify HTTP helpers ----------------
    private Task<List<JsonElement>> GetTracksAsync(IEnumerable<string> ids, CancellationToken ct)
        => GetJsonArrayAsync($"https://api.spotify.com/v1/tracks?ids={string.Join(",", ids)}", "tracks", ct);

    private Task<List<JsonElement>> GetArtistsAsync(IEnumerable<string> ids, CancellationToken ct)
        => GetJsonArrayAsync($"https://api.spotify.com/v1/artists?ids={string.Join(",", ids)}", "artists", ct);

    private async Task<List<JsonElement>> GetJsonArrayAsync(string url, string root, CancellationToken ct)
    {
        SemaphoreSlim? gate = _httpGate;
        if (gate != null) await gate.WaitAsync(ct);
        try
        {
            // Auth per-request (not via DefaultRequestHeaders) so parallel calls don't race
            // on a shared header while another request is refreshing the token.
            using HttpRequestMessage req = new(HttpMethod.Get, url);
            string? token = _token;
            if (token != null)
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using HttpResponseMessage res = await _http.SendAsync(req, ct);

            // Surface the two transient cases so GetWithRetryAsync can react to them.
            if (res.StatusCode == HttpStatusCode.TooManyRequests)
                throw new TransientApiException { RetryAfter = res.Headers.RetryAfter?.Delta };
            if (res.StatusCode == HttpStatusCode.Unauthorized)
                throw new TransientApiException { Unauthorized = true };

            res.EnsureSuccessStatusCode();

            await using Stream s = await res.Content.ReadAsStreamAsync(ct);
            using JsonDocument doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);
            return doc.RootElement
                .GetProperty(root)
                .EnumerateArray()
                .Select(e => e.Clone()) // detach from the JsonDocument before it's disposed
                .ToList();
        }
        finally
        {
            gate?.Release();
        }
    }

    // Bounded retry: refresh the token on 401, honour Retry-After on 429.
    private async Task<T> GetWithRetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        for (int attempt = 1;; attempt++)
        {
            try
            {
                return await action();
            }
            catch (TransientApiException ex) when (attempt <= MaxRetries)
            {
                if (ex.Unauthorized)
                {
                    _log.LogWarning("401 Unauthorized (attempt {Attempt}); refreshing token", attempt);
                    await EnsureTokenAsync(ct, staleToken: _token);
                }
                else
                {
                    TimeSpan delay = ex.RetryAfter is { } ra && ra > TimeSpan.Zero
                        ? (ra < TimeSpan.FromSeconds(30) ? ra : TimeSpan.FromSeconds(30))
                        : TimeSpan.FromSeconds(3);
                    _log.LogWarning("429 TooManyRequests (attempt {Attempt}); sleeping {Delay}", attempt, delay);
                    await Task.Delay(delay, ct);
                }
            }
        }
    }

    private sealed class TransientApiException : Exception
    {
        public TimeSpan? RetryAfter { get; init; }
        public bool Unauthorized { get; init; }
    }
}
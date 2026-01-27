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
}

public class SpotifyCatalogService
{
    private readonly HttpClient _http;
    private readonly AppDbContext _db;
    private readonly SpotifyOptions _opt;
    private readonly ILogger<SpotifyCatalogService> _log; // <-- define logger

    // Services/SpotifyCatalogService.cs

    public SpotifyCatalogService(
        HttpClient http,
        AppDbContext db,
        IOptions<SpotifyOptions> opt,
        ILogger<SpotifyCatalogService> log)
    {
        _http = http;
        _db = db;
        _opt = opt.Value;
        _log = log;

        string id = _opt.ClientId?.Trim() ?? "";
        string idPreview = string.IsNullOrEmpty(id)
            ? "<empty>"
            : id.Substring(0, Math.Min(6, id.Length));

        _log.LogInformation(
            "SpotifyOptions loaded. ClientId starts with: {Prefix}. ClientSecret set: {HasSecret}",
            idPreview,
            !string.IsNullOrWhiteSpace(_opt.ClientSecret));
    }


    static string? TrackIdFromUri(string? uri)
        => string.IsNullOrWhiteSpace(uri) ? null : (uri.Split(':') is { Length: 3 } p ? p[2] : null);

    // -------------------- PUBLIC: full backfill --------------------
    public async Task<int> BackfillAllAsync(CancellationToken ct = default)
    {
        using IDisposable? scope = _log.BeginScope("job:{JobId}", Guid.NewGuid().ToString("N"));
        _log.LogInformation("Backfill started");

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

        _log.LogInformation("Backfill finished. Inserted {Inserted} tracks", inserted);
        return inserted;
    }

    // -------------------- PUBLIC: delta backfill -------------------
    public async Task<int> BackfillDeltaAsync(CancellationToken ct = default)
    {
        using IDisposable? scope = _log.BeginScope("job:{JobId}", Guid.NewGuid().ToString("N"));
        _log.LogInformation("Delta backfill started");

        await BackfillTrackIdsOnSpotifyAsync(ct);

        List<string> unknownTrackIds = await _db.Spotify
            .Where(s => s.TrackId != null)
            .Where(s => !_db.TracksCatalog.Any(t => t.TrackId == s.TrackId))
            .Select(s => s.TrackId!)
            .Distinct()
            .ToListAsync(ct);

        _log.LogInformation("Delta: {Count} new unknown track IDs", unknownTrackIds.Count);

        int inserted = await EnrichTracksAsync(unknownTrackIds, ct);

        _log.LogInformation("Delta backfill finished. Inserted {Inserted} tracks", inserted);
        return inserted;
    }

    // -------------------- fill TrackId column once / when new rows appear
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

    // -------------------- core enrichment with batching + 429 handling
    private async Task<int> EnrichTracksAsync(List<string> trackIds, CancellationToken ct)
    {
        int total = trackIds.Count;
        if (total == 0)
        {
            _log.LogInformation("No work to do");
            return 0;
        }

        _log.LogInformation("Enriching {Total} tracks in batches of 50...", total);

        string token = await GetAccessTokenAsync(ct);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        int inserted = 0;
        int processed = 0;
        int batchNo = 0;
        int totalBatches = (int)Math.Ceiling(total / 50.0);

        foreach (List<string> batch in Batch(trackIds, 50))
        {
            batchNo++;
            int batchSize = batch.Count;
            _log.LogInformation("Batch {Batch}/{TotalBatches}, size {Size}", batchNo, totalBatches, batchSize);

            List<JsonElement> tracks = await GetWithRetryAsync(() => GetTracksAsync(batch, ct), ct);
            _log.LogInformation("Batch {Batch}: tracks API returned {TrackCount}", batchNo, tracks.Count);

            List<TrackCatalog> newTracks = new List<TrackCatalog>();
            List<TrackArtist> newTrackArtists = new List<TrackArtist>();
            HashSet<string> artistIds = new HashSet<string>();

            foreach (JsonElement t in tracks)
            {
                if (t.ValueKind == JsonValueKind.Undefined || t.ValueKind == JsonValueKind.Null) continue;

                string trackId = t.GetProperty("id").GetString()!;
                string? name = t.GetProperty("name").GetString();
                JsonElement album = t.GetProperty("album");
                string? albumId = album.TryGetProperty("id", out JsonElement aid) ? aid.GetString() : null;
                string? albumName = album.TryGetProperty("name", out JsonElement an) ? an.GetString() : null;

                if (!await _db.TracksCatalog.AnyAsync(x => x.TrackId == trackId, ct))
                {
                    newTracks.Add(new TrackCatalog
                    {
                        TrackId = trackId,
                        Name = name,
                        AlbumId = albumId,
                        AlbumName = albumName,
                        FetchedAtUtc = DateTime.UtcNow
                    });
                    inserted++;
                }

                foreach (JsonElement a in t.GetProperty("artists").EnumerateArray())
                {
                    string artistId = a.GetProperty("id").GetString()!;
                    artistIds.Add(artistId);

                    // we’ll add these AFTER artists exist
                    if (!await _db.TrackArtists.AnyAsync(x => x.TrackId == trackId && x.ArtistId == artistId, ct))
                        newTrackArtists.Add(new TrackArtist { TrackId = trackId, ArtistId = artistId });
                }
            }

            // 1) save tracks first (so FK TrackArtists→TrackCatalog will be valid)
            if (newTracks.Count > 0)
            {
                _db.TracksCatalog.AddRange(newTracks);
                await _db.SaveChangesAsync(ct);
            }

            // 2) make sure all artists exist, then save them + their genres
            List<string> existingArtistIds = await _db.ArtistsCatalog
                .Where(a => artistIds.Contains(a.ArtistId))
                .Select(a => a.ArtistId)
                .ToListAsync(ct);

            List<string> unknownArtists = artistIds.Except(existingArtistIds).ToList();

            foreach (List<string> aBatch in Batch(unknownArtists, 50))
            {
                List<JsonElement> artists = await GetWithRetryAsync(() => GetArtistsAsync(aBatch, ct), ct);

                List<ArtistCatalog> newArtists = new List<ArtistCatalog>();
                List<ArtistGenre> newGenres = new List<ArtistGenre>();

                foreach (JsonElement a in artists)
                {
                    if (a.ValueKind == JsonValueKind.Undefined || a.ValueKind == JsonValueKind.Null) continue;

                    string artistId = a.GetProperty("id").GetString()!;
                    string? name = a.GetProperty("name").GetString();

                    if (!await _db.ArtistsCatalog.AnyAsync(x => x.ArtistId == artistId, ct))
                        newArtists.Add(new ArtistCatalog
                            { ArtistId = artistId, Name = name, FetchedAtUtc = DateTime.UtcNow });

                    if (a.TryGetProperty("genres", out JsonElement g) && g.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement node in g.EnumerateArray())
                        {
                            string? genre = node.GetString();
                            if (!string.IsNullOrWhiteSpace(genre) &&
                                !await _db.ArtistGenres.AnyAsync(x => x.ArtistId == artistId && x.Genre == genre, ct))
                            {
                                newGenres.Add(new ArtistGenre { ArtistId = artistId, Genre = genre! });
                            }
                        }
                    }
                }

                if (newArtists.Count > 0) _db.ArtistsCatalog.AddRange(newArtists);
                if (newGenres.Count > 0) _db.ArtistGenres.AddRange(newGenres);
                await _db.SaveChangesAsync(ct);
            }

            // 3) NOW insert the track-artist links (FK will succeed)
            if (newTrackArtists.Count > 0)
            {
                _db.TrackArtists.AddRange(newTrackArtists);
                await _db.SaveChangesAsync(ct);
            }

            // 4) build weights for just-inserted tracks
            foreach (TrackCatalog track in newTracks)
                await BuildWeightsForTrackAsync(track.TrackId, ct);

            _log.LogInformation("Batch {Batch}: built genre weights for {Weighted} tracks", batchNo, newTracks.Count);

            processed += batchSize;
            _log.LogInformation("Progress {Processed}/{Total} ({Percent:P1})",
                processed, total, processed / (double)total);

            await Task.Delay(150, ct); // pacing
        }

        _log.LogInformation("Enrichment complete. Total inserted tracks: {Inserted}", inserted);
        return inserted;
    }

    private async Task BuildWeightsForTrackAsync(string trackId, CancellationToken ct)
    {
        List<string> artistIds = await _db.TrackArtists
            .Where(t => t.TrackId == trackId)
            .Select(t => t.ArtistId)
            .ToListAsync(ct);

        if (artistIds.Count == 0) return;

        int aCount = artistIds.Count;
        Dictionary<string, double> weights = new Dictionary<string, double>();

        foreach (string a in artistIds)
        {
            List<string> genres = await _db.ArtistGenres
                .Where(g => g.ArtistId == a)
                .Select(g => g.Genre)
                .ToListAsync(ct);

            if (genres.Count == 0) continue;

            double share = 1.0 / aCount / genres.Count;
            foreach (string g in genres)
                weights[g] = (weights.TryGetValue(g, out double w) ? w : 0) + share;
        }

        IQueryable<TrackGenreWeight> existing = _db.TrackGenreWeights.Where(x => x.TrackId == trackId);
        _db.TrackGenreWeights.RemoveRange(existing);
        _db.TrackGenreWeights.AddRange(
            weights.Select(kv => new TrackGenreWeight
            {
                TrackId = trackId,
                Genre = kv.Key,
                Weight = kv.Value,
                BuiltAtUtc = DateTime.UtcNow
            })
        );
        await _db.SaveChangesAsync(ct);
    }

    // ---------------- Spotify HTTP helpers + 429 backoff ----------------
    private async Task<string> GetAccessTokenAsync(CancellationToken ct)
    {
        string id = _opt.ClientId?.Trim() ?? "";
        string secret = _opt.ClientSecret?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Spotify ClientId/ClientSecret are not configured.");

        using HttpRequestMessage
            req = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

        string basic = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{id}:{secret}"));
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        });

        HttpResponseMessage res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            string body = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Token request failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {body}");
        }

        using Stream s = await res.Content.ReadAsStreamAsync(ct);
        using JsonDocument json = await JsonDocument.ParseAsync(s, cancellationToken: ct);
        return json.RootElement.GetProperty("access_token").GetString()!;
    }


    private async Task<List<JsonElement>> GetTracksAsync(IEnumerable<string> ids, CancellationToken ct)
        => await GetJsonArrayAsync($"https://api.spotify.com/v1/tracks?ids={string.Join(",", ids)}", "tracks", ct);

    private async Task<List<JsonElement>> GetArtistsAsync(IEnumerable<string> ids, CancellationToken ct)
        => await GetJsonArrayAsync($"https://api.spotify.com/v1/artists?ids={string.Join(",", ids)}", "artists", ct);

    private async Task<List<JsonElement>> GetJsonArrayAsync(string url, string root, CancellationToken ct)
    {
        using HttpResponseMessage res = await _http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        await using Stream s = await res.Content.ReadAsStreamAsync(ct);
        using JsonDocument doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);

        return doc.RootElement
            .GetProperty(root)
            .EnumerateArray()
            .Select(e => e.Clone()) // <- crucial
            .ToList();
    }


    private async Task<T> GetWithRetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        int attempt = 0;
        while (true)
        {
            attempt++;
            try
            {
                return await action();
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                TimeSpan delay = TimeSpan.FromSeconds(3);
                _log.LogWarning(ex, "429 TooManyRequests (attempt {Attempt}). Sleeping {Delay}...", attempt, delay);
                await Task.Delay(delay, ct);
            }
        }
    }

    private static IEnumerable<List<T>> Batch<T>(IEnumerable<T> src, int size)
    {
        List<T> list = new List<T>(size);
        foreach (T x in src)
        {
            list.Add(x);
            if (list.Count == size)
            {
                yield return list;
                list = new List<T>(size);
            }
        }

        if (list.Count > 0) yield return list;
    }
}
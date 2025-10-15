using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

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
    private readonly ILogger<SpotifyCatalogService> _log;  // <-- define logger

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

        // 👇 safe sanity log
        var id = _opt.ClientId?.Trim() ?? "";
        var idPreview = string.IsNullOrEmpty(id)
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
        using var scope = _log.BeginScope("job:{JobId}", Guid.NewGuid().ToString("N"));
        _log.LogInformation("Backfill started");

        await BackfillTrackIdsOnSpotifyAsync(ct);

        // MySQL-friendly NOT EXISTS (instead of Except)
        var unknownTrackIds = await _db.Spotify
            .Where(s => s.TrackId != null)
            .Where(s => !_db.TracksCatalog.Any(t => t.TrackId == s.TrackId))
            .Select(s => s.TrackId!)
            .Distinct()
            .ToListAsync(ct);

        _log.LogInformation("Found {Count} unknown track IDs", unknownTrackIds.Count);

        var inserted = await EnrichTracksAsync(unknownTrackIds, ct);

        _log.LogInformation("Backfill finished. Inserted {Inserted} tracks", inserted);
        return inserted;
    }

    // -------------------- PUBLIC: delta backfill -------------------
    public async Task<int> BackfillDeltaAsync(CancellationToken ct = default)
    {
        using var scope = _log.BeginScope("job:{JobId}", Guid.NewGuid().ToString("N"));
        _log.LogInformation("Delta backfill started");

        await BackfillTrackIdsOnSpotifyAsync(ct);

        var unknownTrackIds = await _db.Spotify
            .Where(s => s.TrackId != null)
            .Where(s => !_db.TracksCatalog.Any(t => t.TrackId == s.TrackId))
            .Select(s => s.TrackId!)
            .Distinct()
            .ToListAsync(ct);

        _log.LogInformation("Delta: {Count} new unknown track IDs", unknownTrackIds.Count);

        var inserted = await EnrichTracksAsync(unknownTrackIds, ct);

        _log.LogInformation("Delta backfill finished. Inserted {Inserted} tracks", inserted);
        return inserted;
    }

    // -------------------- fill TrackId column once / when new rows appear
    private async Task BackfillTrackIdsOnSpotifyAsync(CancellationToken ct)
    {
        var rows = await _db.Spotify
            .Where(s => s.spotify_track_uri != null && s.TrackId == null)
            .ToListAsync(ct);

        foreach (var r in rows) r.TrackId = TrackIdFromUri(r.spotify_track_uri);

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
        var total = trackIds.Count;
        if (total == 0) { _log.LogInformation("No work to do"); return 0; }

        _log.LogInformation("Enriching {Total} tracks in batches of 50...", total);

        var token = await GetAccessTokenAsync(ct);
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        int inserted = 0;
        int processed = 0;
        int batchNo = 0;
        int totalBatches = (int)Math.Ceiling(total / 50.0);

        foreach (var batch in Batch(trackIds, 50))
        {
            batchNo++;
            var batchSize = batch.Count;
            _log.LogInformation("Batch {Batch}/{TotalBatches}, size {Size}", batchNo, totalBatches, batchSize);

            var tracks = await GetWithRetryAsync(() => GetTracksAsync(batch, ct), ct);
            _log.LogInformation("Batch {Batch}: tracks API returned {TrackCount}", batchNo, tracks.Count);

            var newTracks = new List<TrackCatalog>();
            var newTrackArtists = new List<TrackArtist>();
            var artistIds = new HashSet<string>();

            foreach (var t in tracks)
            {
                if (t.ValueKind == JsonValueKind.Undefined || t.ValueKind == JsonValueKind.Null) continue;

                var trackId = t.GetProperty("id").GetString()!;
                var name = t.GetProperty("name").GetString();
                var album = t.GetProperty("album");
                var albumId = album.TryGetProperty("id", out var aid) ? aid.GetString() : null;
                var albumName = album.TryGetProperty("name", out var an) ? an.GetString() : null;

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

                foreach (var a in t.GetProperty("artists").EnumerateArray())
                {
                    var artistId = a.GetProperty("id").GetString()!;
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
            var existingArtistIds = await _db.ArtistsCatalog
                .Where(a => artistIds.Contains(a.ArtistId))
                .Select(a => a.ArtistId)
                .ToListAsync(ct);

            var unknownArtists = artistIds.Except(existingArtistIds).ToList();

            foreach (var aBatch in Batch(unknownArtists, 50))
            {
                var artists = await GetWithRetryAsync(() => GetArtistsAsync(aBatch, ct), ct);

                var newArtists = new List<ArtistCatalog>();
                var newGenres = new List<ArtistGenre>();

                foreach (var a in artists)
                {
                    if (a.ValueKind == JsonValueKind.Undefined || a.ValueKind == JsonValueKind.Null) continue;

                    var artistId = a.GetProperty("id").GetString()!;
                    var name = a.GetProperty("name").GetString();

                    if (!await _db.ArtistsCatalog.AnyAsync(x => x.ArtistId == artistId, ct))
                        newArtists.Add(new ArtistCatalog { ArtistId = artistId, Name = name, FetchedAtUtc = DateTime.UtcNow });

                    if (a.TryGetProperty("genres", out var g) && g.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var node in g.EnumerateArray())
                        {
                            var genre = node.GetString();
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
            foreach (var track in newTracks)
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
        var artistIds = await _db.TrackArtists
            .Where(t => t.TrackId == trackId)
            .Select(t => t.ArtistId)
            .ToListAsync(ct);

        if (artistIds.Count == 0) return;

        var aCount = artistIds.Count;
        var weights = new Dictionary<string, double>();

        foreach (var a in artistIds)
        {
            var genres = await _db.ArtistGenres
                .Where(g => g.ArtistId == a)
                .Select(g => g.Genre)
                .ToListAsync(ct);

            if (genres.Count == 0) continue;

            var share = 1.0 / aCount / genres.Count;
            foreach (var g in genres)
                weights[g] = (weights.TryGetValue(g, out var w) ? w : 0) + share;
        }

        var existing = _db.TrackGenreWeights.Where(x => x.TrackId == trackId);
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
        var id = _opt.ClientId?.Trim() ?? "";
        var secret = _opt.ClientSecret?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("Spotify ClientId/ClientSecret are not configured.");

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");

        var basic = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{id}:{secret}"));
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
        req.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials"
        });

        var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException($"Token request failed: {(int)res.StatusCode} {res.ReasonPhrase}. Body: {body}");
        }

        using var s = await res.Content.ReadAsStreamAsync(ct);
        using var json = await JsonDocument.ParseAsync(s, cancellationToken: ct);
        return json.RootElement.GetProperty("access_token").GetString()!;
    }



    private async Task<List<JsonElement>> GetTracksAsync(IEnumerable<string> ids, CancellationToken ct)
        => await GetJsonArrayAsync($"https://api.spotify.com/v1/tracks?ids={string.Join(",", ids)}", "tracks", ct);

    private async Task<List<JsonElement>> GetArtistsAsync(IEnumerable<string> ids, CancellationToken ct)
        => await GetJsonArrayAsync($"https://api.spotify.com/v1/artists?ids={string.Join(",", ids)}", "artists", ct);

    private async Task<List<JsonElement>> GetJsonArrayAsync(string url, string root, CancellationToken ct)
    {
        using var res = await _http.GetAsync(url, ct);
        res.EnsureSuccessStatusCode();
        await using var s = await res.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(s, cancellationToken: ct);

        return doc.RootElement
                  .GetProperty(root)
                  .EnumerateArray()
                  .Select(e => e.Clone())   // <- crucial
                  .ToList();
    }


    private async Task<T> GetWithRetryAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        var attempt = 0;
        while (true)
        {
            attempt++;
            try { return await action(); }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var delay = TimeSpan.FromSeconds(3);
                _log.LogWarning(ex, "429 TooManyRequests (attempt {Attempt}). Sleeping {Delay}...", attempt, delay);
                await Task.Delay(delay, ct);
            }
        }
    }

    private static IEnumerable<List<T>> Batch<T>(IEnumerable<T> src, int size)
    {
        var list = new List<T>(size);
        foreach (var x in src)
        {
            list.Add(x);
            if (list.Count == size) { yield return list; list = new List<T>(size); }
        }
        if (list.Count > 0) yield return list;
    }
}

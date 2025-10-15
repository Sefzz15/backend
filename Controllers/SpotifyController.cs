using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    private readonly SpotifyService _spotifyService;
    private readonly AppDbContext _context;

    public SpotifyController(SpotifyService spotifyService, AppDbContext db)
    {
        _spotifyService = spotifyService;
        _context = db;
    }

    // List + filters
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<Spotify>>> GetAllSpotify(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string sortDirection = "asc",
        // Filters (optional)
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? type = null,
        [FromQuery] int? minMs = null,
        [FromQuery] string? query = null)
    {
        if (page <= 0 || pageSize <= 0)
            return BadRequest("Page and pageSize must be greater than zero.");

        var filters = new SpotifyFilterParams(from, to, type, minMs, query);
        var result = await _spotifyService.GetSpotifyPageWithMetadata(page, pageSize, sortColumn, sortDirection, filters);
        return Ok(result);
    }

    // CRUD
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpotifyById(int id)
    {
        var spotify = await _spotifyService.GetSpotifyById(id);
        if (spotify == null) return NotFound();
        return Ok(spotify);
    }

    [HttpPost]
    public async Task<IActionResult> AddSpotify([FromBody] Spotify spotify)
    {
        await _spotifyService.AddSpotify(spotify);
        return CreatedAtAction(nameof(GetSpotifyById), new { id = spotify.Id }, spotify);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSpotify(int id, [FromBody] Spotify spotify)
    {
        if (id != spotify.Id) return BadRequest();
        await _spotifyService.UpdateSpotify(spotify);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpotify(int id)
    {
        await _spotifyService.DeleteSpotify(id);
        return NoContent();
    }

    // --- Summaries ---
    [HttpGet("summary/top-tracks")]
    public async Task<IActionResult> GetTopTracks(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10,
        [FromQuery] int minMs = 30000,
        [FromQuery] string countBy = "time", // "time" | "plays"
        [FromQuery] string? type = "songs",
        [FromQuery] string? query = null)
    {
        var filters = new SpotifyFilterParams(from, to, type, minMs, query);
        var data = await _spotifyService.GetTopTracksAsync(limit, countBy, filters);
        return Ok(data);
    }

    [HttpGet("summary/top-artists")]
    public async Task<IActionResult> GetTopArtists(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10,
        [FromQuery] int minMs = 30000,
        [FromQuery] string countBy = "time", // "time" | "plays"
        [FromQuery] string? type = "songs",
        [FromQuery] string? query = null)
    {
        var filters = new SpotifyFilterParams(from, to, type, minMs, query);
        var data = await _spotifyService.GetTopArtistsAsync(limit, countBy, filters);
        return Ok(data);
    }

    [HttpGet("summary/heatmap")]
    public async Task<IActionResult> GetHeatmap(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int minMs = 30000,
        [FromQuery] string? type = null,
        [FromQuery] string? query = null)
    {
        var filters = new SpotifyFilterParams(from, to, type, minMs, query);
        var cells = await _spotifyService.GetHeatmapAsync(filters);
        return Ok(cells);
    }

    [HttpPost("enrich/backfill")]
    public async Task<IActionResult> EnrichBackfill([FromServices] SpotifyCatalogService svc, CancellationToken ct)
    {
        var inserted = await svc.BackfillAllAsync(ct);
        return Ok(new { inserted });
    }

    [HttpPost("enrich/delta")]
    public async Task<IActionResult> EnrichDelta([FromServices] SpotifyCatalogService svc, CancellationToken ct)
    {
        var inserted = await svc.BackfillDeltaAsync(ct);
        return Ok(new { inserted });
    }

    [HttpGet("summary/top-genres")]
    public async Task<IActionResult> GetTopGenres(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int limit = 10,
        [FromQuery] int minMs = 30000,
        [FromQuery] string countBy = "time", // "time" | "plays"
        [FromQuery] string? type = null,
        [FromQuery] string? query = null)
    {
        var f = new SpotifyFilterParams(from, to, type, minMs, query);

        var plays = SpotifyServiceApplyFilters(
                        _context.Spotify.AsNoTracking().Where(x => x.TrackId != null), f);

        // Join plays -> weights and aggregate server-side to an anonymous type
        var agg = await (
            from p in plays
            join w in _context.TrackGenreWeights on p.TrackId equals w.TrackId
            group new { p, w } by w.Genre into g
            select new
            {
                Genre = g.Key,
                PlaysWeighted = g.Sum(x => x.w.Weight),                
                TotalMsWeightedD = g.Sum(x => x.p.ms_played * x.w.Weight) 
            })
            .ToListAsync();

        // Order + take in memory and map to DTO (note: rounding to long)
        var ordered = countBy.Equals("plays", StringComparison.OrdinalIgnoreCase)
            ? agg.OrderByDescending(x => x.PlaysWeighted)
            : agg.OrderByDescending(x => x.TotalMsWeightedD);

        var result = ordered
            .Take(limit)
            .Select(x => new TopGenreDto(x.Genre, x.PlaysWeighted, (long)Math.Round(x.TotalMsWeightedD)))
            .ToList();

        return Ok(result);
    }

    private IQueryable<Spotify> SpotifyServiceQueryable() => _context.Spotify.AsNoTracking();

    private static IQueryable<Spotify> SpotifyServiceApplyFilters(IQueryable<Spotify> q, SpotifyFilterParams f)
    {
        if (f.From.HasValue) q = q.Where(x => x.ts >= f.From.Value);
        if (f.To.HasValue) q = q.Where(x => x.ts < f.To.Value);
        if (f.MinMs.HasValue) q = q.Where(x => x.ms_played >= f.MinMs.Value);
        if (!string.IsNullOrWhiteSpace(f.Type))
        {
            switch (f.Type!.ToLowerInvariant())
            {
                case "songs": q = q.Where(x => x.spotify_track_uri != null); break;
                case "podcasts": q = q.Where(x => x.spotify_episode_uri != null); break;
                case "audiobooks": q = q.Where(x => x.audiobook_uri != null); break;
            }
        }
        if (!string.IsNullOrWhiteSpace(f.Query))
        {
            var term = f.Query!.Trim();
            q = q.Where(x =>
                (x.master_metadata_track_name != null && EF.Functions.Like(x.master_metadata_track_name, $"%{term}%")) ||
                (x.master_metadata_album_artist_name != null && EF.Functions.Like(x.master_metadata_album_artist_name, $"%{term}%")) ||
                (x.episode_name != null && EF.Functions.Like(x.episode_name, $"%{term}%")) ||
                (x.episode_show_name != null && EF.Functions.Like(x.episode_show_name, $"%{term}%"))
            );
        }
        return q;
    }
}

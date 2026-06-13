using backend.Data;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace backend.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyController(SpotifyQueryService spotifyService, AppDbContext db) : ControllerBase
{
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

        SpotifyFilterParams filters = new SpotifyFilterParams(from, to, type, minMs, query);
        PaginatedResponse<Spotify> result =
            await spotifyService.GetSpotifyPageWithMetadata(page, pageSize, sortColumn, sortDirection, filters);
        return Ok(result);
    }

    // CRUD
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSpotifyById(int id)
    {
        Spotify? spotify = await spotifyService.GetSpotifyById(id);
        if (spotify == null) return NotFound();
        return Ok(spotify);
    }

    [HttpPost]
    public async Task<IActionResult> AddSpotify([FromBody] Spotify spotify)
    {
        await spotifyService.AddSpotify(spotify);
        return CreatedAtAction(nameof(GetSpotifyById), new { id = spotify.Id }, spotify);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSpotify(int id, [FromBody] Spotify spotify)
    {
        if (id != spotify.Id) return BadRequest();
        await spotifyService.UpdateSpotify(spotify);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSpotify(int id)
    {
        await spotifyService.DeleteSpotify(id);
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
        SpotifyFilterParams filters = new SpotifyFilterParams(from, to, type, minMs, query);
        IReadOnlyList<TopTrackDto> data = await spotifyService.GetTopTracksAsync(limit, countBy, filters);
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
        SpotifyFilterParams filters = new SpotifyFilterParams(from, to, type, minMs, query);
        IReadOnlyList<TopArtistDto> data = await spotifyService.GetTopArtistsAsync(limit, countBy, filters);
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
        SpotifyFilterParams filters = new SpotifyFilterParams(from, to, type, minMs, query);
        IReadOnlyList<HeatCellDto> cells = await spotifyService.GetHeatmapAsync(filters);
        return Ok(cells);
    }

    [HttpPost("enrich/backfill")]
    public async Task<IActionResult> EnrichBackfill([FromServices] SpotifyEnricher svc, CancellationToken ct)
    {
        int inserted = await svc.BackfillAllAsync(ct);
        return Ok(new { inserted });
    }

    [HttpPost("enrich/delta")]
    public async Task<IActionResult> EnrichDelta([FromServices] SpotifyEnricher svc, CancellationToken ct)
    {
        int inserted = await svc.BackfillDeltaAsync(ct);
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
        SpotifyFilterParams f = new SpotifyFilterParams(from, to, type, minMs, query);

        IQueryable<Spotify> plays = SpotifyQueryService.ApplyFilters(
            db.Spotify.AsNoTracking().Where(x => x.TrackId != null), f);

        // Join plays -> weights and aggregate server-side to an anonymous type
        var agg = await (
                from p in plays
                join w in db.TrackGenreWeights on p.TrackId equals w.TrackId
                group new { p, w } by w.Genre
                into g
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

        List<TopGenreDto> result = ordered
            .Take(limit)
            .Select(x => new TopGenreDto(x.Genre, x.PlaysWeighted, (long)Math.Round(x.TotalMsWeightedD)))
            .ToList();

        return Ok(result);
    }
}
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/spotify")]
public class SpotifyController : ControllerBase
{
    private readonly SpotifyService _spotifyService;

    public SpotifyController(SpotifyService spotifyService)
    {
        _spotifyService = spotifyService;
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
}

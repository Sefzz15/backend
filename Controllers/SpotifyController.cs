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

    [HttpGet]
    public async Task<IActionResult> GetAllSpotify(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        [FromQuery] string sortColumn = null,
        [FromQuery] string sortDirection = "asc")
    {
        if (page <= 0 || pageSize <= 0)
        {
            return BadRequest("Page and pageSize must be greater than zero.");
        }

        var result = await _spotifyService.GetSpotifyPageWithMetadata(page, pageSize, sortColumn, sortDirection);

        return Ok(result);
    }



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
}
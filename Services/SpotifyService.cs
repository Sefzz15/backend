using backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace backend.Services;

public class SpotifyService
{
    private readonly AppDbContext _context;
    public SpotifyService(AppDbContext context) => _context = context;

    // Sorting whitelist
    private static readonly HashSet<string> SortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Spotify.Id), nameof(Spotify.ts), nameof(Spotify.ms_played),
        nameof(Spotify.master_metadata_track_name),
        nameof(Spotify.master_metadata_album_artist_name),
        nameof(Spotify.episode_name), nameof(Spotify.episode_show_name)
    };

    private IQueryable<Spotify> BaseQuery => _context.Spotify.AsNoTracking();

    private static IQueryable<Spotify> ApplyFilters(IQueryable<Spotify> q, SpotifyFilterParams f)
    {
        if (f.From.HasValue) q = q.Where(x => x.ts >= f.From.Value);
        if (f.To.HasValue)   q = q.Where(x => x.ts <  f.To.Value);
        if (f.MinMs.HasValue) q = q.Where(x => x.ms_played >= f.MinMs.Value);

        if (!string.IsNullOrWhiteSpace(f.Type))
        {
            switch (f.Type!.ToLowerInvariant())
            {
                case "songs":      q = q.Where(x => x.spotify_track_uri     != null); break;
                case "podcasts":   q = q.Where(x => x.spotify_episode_uri    != null); break;
                case "audiobooks": q = q.Where(x => x.audiobook_uri          != null); break;
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

    public async Task<PaginatedResponse<Spotify>> GetSpotifyPageWithMetadata(
        int page, int pageSize, string? sortColumn, string sortDirection, SpotifyFilterParams filters)
    {
        var q = ApplyFilters(BaseQuery, filters);

        if (!string.IsNullOrWhiteSpace(sortColumn) && SortableColumns.Contains(sortColumn))
        {
            bool desc = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);
            q = desc ? q.OrderByDescending(ToKeySelector(sortColumn)) : q.OrderBy(ToKeySelector(sortColumn));
        }
        else
        {
            q = q.OrderByDescending(x => x.ts);
        }

        var totalItems = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedResponse<Spotify>
        {
            Items = items,
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize)
        };
    }

    // Backward-compatible overload (no filters)
    public Task<PaginatedResponse<Spotify>> GetSpotifyPageWithMetadata(
        int page, int pageSize, string? sortColumn, string sortDirection)
        => GetSpotifyPageWithMetadata(page, pageSize, sortColumn, sortDirection,
           new SpotifyFilterParams(null, null, null, null, null));

    private static Expression<Func<Spotify, object>> ToKeySelector(string column) => column switch
    {
        nameof(Spotify.Id) => x => x.Id,
        nameof(Spotify.ts) => x => x.ts,
        nameof(Spotify.ms_played) => x => x.ms_played,
        nameof(Spotify.master_metadata_track_name) => x => x.master_metadata_track_name!,
        nameof(Spotify.master_metadata_album_artist_name) => x => x.master_metadata_album_artist_name!,
        nameof(Spotify.episode_name) => x => x.episode_name!,
        nameof(Spotify.episode_show_name) => x => x.episode_show_name!,
        _ => x => x.ts
    };

    // CRUD
    public Task<List<Spotify>> GetAllSpotify() => _context.Spotify.AsNoTracking().ToListAsync();
    public Task<Spotify?> GetSpotifyById(int id) => _context.Spotify.FindAsync(id).AsTask();
    public async Task AddSpotify(Spotify spotify) { _context.Spotify.Add(spotify); await _context.SaveChangesAsync(); }
    public async Task UpdateSpotify(Spotify spotify) { _context.Spotify.Update(spotify); await _context.SaveChangesAsync(); }
    public async Task DeleteSpotify(int id)
    {
        var spotify = await _context.Spotify.FindAsync(id);
        if (spotify is not null) { _context.Spotify.Remove(spotify); await _context.SaveChangesAsync(); }
    }

    // Aggregations
public async Task<IReadOnlyList<TopTrackDto>> GetTopTracksAsync(int limit, string countBy, SpotifyFilterParams f)
{
    var q = ApplyFilters(BaseQuery, f)
        .Where(x => x.spotify_track_uri != null && x.master_metadata_track_name != null);

    // 1) Aggregate to anonymous type server-side
    var agg = await q
        .GroupBy(x => new { x.master_metadata_track_name, x.master_metadata_album_artist_name, x.spotify_track_uri })
        .Select(g => new
        {
            TrackName  = g.Key.master_metadata_track_name!,
            ArtistName = g.Key.master_metadata_album_artist_name!,
            Uri        = g.Key.spotify_track_uri!,
            Plays      = g.Count(),
            TotalMs    = g.Sum(x => (long)x.ms_played)
        })
        .OrderByDescending(x => countBy.Equals("plays", StringComparison.OrdinalIgnoreCase) ? x.Plays : x.TotalMs)
        .ThenBy(x => x.TrackName)
        .Take(limit)
        .ToListAsync();

    // 2) Map to DTOs client-side (EF translation no longer needed)
    return agg.Select(x => new TopTrackDto(x.TrackName, x.ArtistName, x.Uri, x.Plays, x.TotalMs)).ToList();
}

public async Task<IReadOnlyList<TopArtistDto>> GetTopArtistsAsync(int limit, string countBy, SpotifyFilterParams f)
{
    var baseQ = ApplyFilters(BaseQuery, f).Where(x => x.master_metadata_album_artist_name != null);

    // 1) First aggregate (plays/totalMs) server-side
    var agg = await baseQ
        .GroupBy(x => x.master_metadata_album_artist_name)
        .Select(g => new
        {
            ArtistName = g.Key!,
            Plays   = g.Count(),
            TotalMs = g.Sum(x => (long)x.ms_played)
        })
        .OrderByDescending(x => countBy.Equals("plays", StringComparison.OrdinalIgnoreCase) ? x.Plays : x.TotalMs)
        .ThenBy(x => x.ArtistName)
        .Take(limit)
        .ToListAsync();

    // 2) Compute unique track counts for those top artists, server-side via two-level grouping
    var artistNames = agg.Select(a => a.ArtistName).ToList();

    var uniqueCounts = await ApplyFilters(BaseQuery, f)
        .Where(x => x.master_metadata_album_artist_name != null && artistNames.Contains(x.master_metadata_album_artist_name))
        .GroupBy(x => new { x.master_metadata_album_artist_name, x.master_metadata_track_name })
        .Select(g => new { ArtistName = g.Key.master_metadata_album_artist_name! })
        .GroupBy(x => x.ArtistName)
        .Select(g => new { ArtistName = g.Key, UniqueTracks = g.Count() })
        .ToListAsync();

    var uniqueMap = uniqueCounts.ToDictionary(x => x.ArtistName, x => x.UniqueTracks);

    // 3) Map to DTOs
    return agg.Select(a => new TopArtistDto(
        a.ArtistName,
        a.Plays,
        a.TotalMs,
        uniqueMap.TryGetValue(a.ArtistName, out var c) ? c : 0
    )).ToList();
}

    public async Task<IReadOnlyList<HeatCellDto>> GetHeatmapAsync(SpotifyFilterParams f)
    {
        var q = ApplyFilters(BaseQuery, f).Select(x => new { x.ts, x.ms_played });

        var data = await q
            .GroupBy(x => new { Date = x.ts.Date, x.ts.Hour })
            .Select(g => new HeatCellDto(
                g.Key.Date,
                g.Key.Hour,
                g.Count(),
                g.Sum(x => (long)x.ms_played)
            ))
            .OrderBy(x => x.Date).ThenBy(x => x.Hour)
            .ToListAsync();

        return data;
    }
}

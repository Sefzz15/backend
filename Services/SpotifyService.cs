using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
public class SpotifyService
{
    private readonly AppDbContext _context;

    public SpotifyService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Spotify>> GetAllSpotify()
    {
        return await _context.Spotify.ToListAsync();
    }

    public async Task<Spotify?> GetSpotifyById(int id)
    {
        return await _context.Spotify.FindAsync(id);
    }


    public async Task AddSpotify(Spotify spotify)
    {
        _context.Spotify.Add(spotify);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSpotify(Spotify spotify)
    {
        _context.Spotify.Update(spotify);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSpotify(int id)
    {
        var spotify = await _context.Spotify.FindAsync(id);
        if (spotify != null)
        {
            _context.Spotify.Remove(spotify);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<object> GetSpotifyPageWithMetadata(int page, int pageSize, string sortColumn, string sortDirection)
    {
        IQueryable<Spotify> query = _context.Spotify;

        if (!string.IsNullOrWhiteSpace(sortColumn))
        {
            bool ascending = sortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase);
            query = query.OrderBy($"{sortColumn} {sortDirection}");
        }
        else
        {
            query = query.OrderBy(s => s.Id); // default sort
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new
        {
            TotalItems = totalItems,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            Items = items
        };
    }



}
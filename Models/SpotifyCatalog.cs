using System.ComponentModel.DataAnnotations;

public class TrackCatalog
{
    [Key] public string TrackId { get; set; } = null!;
    public string? Name { get; set; }
    public string? AlbumId { get; set; }
    public string? AlbumName { get; set; }
    public DateTime FetchedAtUtc { get; set; }
}

public class ArtistCatalog
{
    [Key] public string ArtistId { get; set; } = null!;
    public string? Name { get; set; }
    public DateTime FetchedAtUtc { get; set; }

    public ICollection<ArtistGenre> Genres { get; set; } = new List<ArtistGenre>();
    public ICollection<TrackArtist> TrackArtists { get; set; } = new List<TrackArtist>();
}

public class TrackArtist
{
    public string TrackId { get; set; } = null!;
    public string ArtistId { get; set; } = null!;

    public TrackCatalog Track { get; set; } = null!;
    public ArtistCatalog Artist { get; set; } = null!;
}

public class ArtistGenre
{
    public string ArtistId { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public ArtistCatalog Artist { get; set; } = null!;
}

// Precomputed per-track genre weights (sum == 1.0 per TrackId)
public class TrackGenreWeight
{
    public string TrackId { get; set; } = null!;
    public string Genre { get; set; } = null!;
    public double Weight { get; set; }
    public DateTime BuiltAtUtc { get; set; }
}

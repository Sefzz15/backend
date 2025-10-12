public sealed record TopTrackDto(
    string TrackName,
    string ArtistName,
    string SpotifyTrackUri,
    int Plays,
    long TotalMs
);

public sealed record TopArtistDto(
    string ArtistName,
    int Plays,
    long TotalMs,
    int UniqueTracks
);

public sealed record HeatCellDto(
    DateTime Date,
    int Hour,
    int Plays,
    long TotalMs
);

public sealed record SpotifyFilterParams(
    DateTime? From,
    DateTime? To,
    string? Type,   // "songs" | "podcasts" | "audiobooks" | null
    int? MinMs,
    string? Query
);

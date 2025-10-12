namespace YourNamespace;

public record TopTrackDto(string TrackName, string ArtistName, string SpotifyTrackUri, int Plays, long TotalMs);
public record TopArtistDto(string ArtistName, int Plays, long TotalMs, int UniqueTracks);
public record HeatCellDto(DateTime Date, int Hour, int Plays, long TotalMs);
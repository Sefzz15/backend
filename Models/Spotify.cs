using System.ComponentModel.DataAnnotations;

public class Spotify
{
    [Key]
    public int Id { get; set; }  // primary key

    public DateTime Ts { get; set; }
    public string? Platform { get; set; }
    public int MsPlayed { get; set; }
    public string? ConnCountry { get; set; }
    public string? IpAddr { get; set; }
    public string? TrackName { get; set; }
    public string? AlbumArtistName { get; set; }
    public string? AlbumName { get; set; }
    public string? SpotifyTrackUri { get; set; }
    public string? EpisodeName { get; set; }
    public string? EpisodeShowName { get; set; }
    public string? SpotifyEpisodeUri { get; set; }
    public string? AudiobookTitle { get; set; }
    public string? AudiobookUri { get; set; }
    public string? AudiobookChapterUri { get; set; }
    public string? AudiobookChapterTitle { get; set; }
    public string? ReasonStart { get; set; }
    public string? ReasonEnd { get; set; }
    public bool Shuffle { get; set; }
    public bool Skipped { get; set; }
    public bool Offline { get; set; }
    public long OfflineTimestamp { get; set; }
    public bool IncognitoMode { get; set; }
}

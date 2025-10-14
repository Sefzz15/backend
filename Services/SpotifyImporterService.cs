using Newtonsoft.Json;

public static class SpotifyImporter
{
    public static async Task ImportAsync(AppDbContext context, CancellationToken ct = default)
    {
        var files = new[]
        {
            @"C:\Users\Sefzz\Desktop\my_spotify_data\Spotify Extended Streaming History\Streaming_History_Audio_2020-2022_1.json",
            @"C:\Users\Sefzz\Desktop\my_spotify_data\Spotify Extended Streaming History\Streaming_History_Audio_2022_2.json",
            @"C:\Users\Sefzz\Desktop\my_spotify_data\Spotify Extended Streaming History\Streaming_History_Audio_2022-2023_3.json",
            @"C:\Users\Sefzz\Desktop\my_spotify_data\Spotify Extended Streaming History\Streaming_History_Audio_2023-2024_4.json",
            @"C:\Users\Sefzz\Desktop\my_spotify_data\Spotify Extended Streaming History\Streaming_History_Audio_2024_5.json",
            @"C:\Users\Sefzz\Desktop\my_spotify_data\Spotify Extended Streaming History\Streaming_History_Audio_2024-2025_6.json",
            @"C:\Users\Sefzz\Desktop\my_spotify_data\Spotify Extended Streaming History\Streaming_History_Audio_2025_7.json",
            @"C:\Users\Sefzz\Desktop\MyProject\Spotify Extended Streaming History\Streaming_History_Audio_2025_7.json"
        };

        var allEntries = new List<Spotify>();

        foreach (var file in files)
        {
            if (File.Exists(file))
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var entries = JsonConvert.DeserializeObject<List<Spotify>>(json);
                if (entries != null)
                {
                    allEntries.AddRange(entries);
                    Console.WriteLine($"✅ Loaded {entries.Count} entries from: {Path.GetFileName(file)}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ File not found: {file}");
            }
        }

        await context.Spotify.AddRangeAsync(allEntries, ct);
        await context.SaveChangesAsync(ct);
        Console.WriteLine($"🎉 Import completed! Total entries: {allEntries.Count}");
    }
}
//  dotnet run -- --import-spotify 
//  --to import Spotify data
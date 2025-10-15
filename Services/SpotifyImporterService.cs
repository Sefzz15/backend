using Newtonsoft.Json;

public static class SpotifyImporter
{
    // New: pass a directory path
    public static async Task ImportAsync(AppDbContext context, string directory, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory path is required.", nameof(directory));

        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"❌ Directory not found: {directory}");
            return;
        }

        var jsonFiles = Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories);

        var allEntries = new List<Spotify>();
        int fileCount = 0;

        foreach (var file in jsonFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file, ct);
                var entries = JsonConvert.DeserializeObject<List<Spotify>>(json);

                if (entries is { Count: > 0 })
                {
                    allEntries.AddRange(entries);
                    Console.WriteLine($"✅ Loaded {entries.Count} entries from: {Path.GetFileName(file)}");
                }
                else
                {
                    Console.WriteLine($"ℹ️  0 entries in: {Path.GetFileName(file)}");
                }

                fileCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Failed to parse {file}: {ex.Message}");
            }
        }

        if (allEntries.Count == 0)
        {
            Console.WriteLine($"No entries found in {fileCount} files under: {directory}");
            return;
        }

        await context.Spotify.AddRangeAsync(allEntries, ct);
        await context.SaveChangesAsync(ct);

        Console.WriteLine($"🎉 Import completed! Files processed: {fileCount}, total entries: {allEntries.Count}");
    }

    public static Task ImportAsync(AppDbContext context, CancellationToken ct = default)
        => ImportAsync(context, @"C:\Users\Sefzz\Desktop\MyProject\Spotify Extended Streaming History", ct);
}


//  dotnet run -- --import-spotify           || to import Spotify data
// 

// Invoke-RestMethod -Method Post -Uri https://localhost:5000/api/spotify/enrich/backfill   ||  to backfill existing records
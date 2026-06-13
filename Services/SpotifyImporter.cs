using System.Globalization;
using backend.Data;
using backend.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace backend.Services;

public static class SpotifyImporter
{
    // New: pass a directory path
    private static async Task ImportAsync(AppDbContext context, string directory, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(directory))
            throw new ArgumentException("Directory path is required.", nameof(directory));

        if (!Directory.Exists(directory))
        {
            Console.WriteLine($"❌ Directory not found: {directory}");
            return;
        }

        IEnumerable<string> jsonFiles = Directory.EnumerateFiles(directory, "*.json", SearchOption.AllDirectories);

        List<Spotify> allEntries = new List<Spotify>();
        int fileCount = 0;

        foreach (string file in jsonFiles)
        {
            try
            {
                string json = await File.ReadAllTextAsync(file, ct);
                List<Spotify>? entries = JsonConvert.DeserializeObject<List<Spotify>>(json);

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

        // ---- De-dupe so re-imports (or overlapping files) don't double-count plays ----
        // A single stream is identified by its end-timestamp (to the second) + the thing
        // played + how long it played. We seed a set with the keys already in the DB, then
        // only keep entries whose key is new (HashSet.Add is false for repeats — this also
        // collapses duplicates that appear within the loaded files themselves).
        var existing = await context.Spotify
            .AsNoTracking()
            .Select(s => new
            {
                s.ts, s.spotify_track_uri, s.spotify_episode_uri, s.audiobook_uri, s.ms_played
            })
            .ToListAsync(ct);

        HashSet<string> seen = new(existing.Count + allEntries.Count);
        foreach (var r in existing)
            seen.Add(KeyOf(r.ts, r.spotify_track_uri, r.spotify_episode_uri, r.audiobook_uri, r.ms_played));

        List<Spotify> toInsert = new();
        foreach (Spotify e in allEntries)
        {
            string key = KeyOf(e.ts, e.spotify_track_uri, e.spotify_episode_uri, e.audiobook_uri, e.ms_played);
            if (seen.Add(key)) toInsert.Add(e);
        }

        int skipped = allEntries.Count - toInsert.Count;

        if (toInsert.Count == 0)
        {
            Console.WriteLine(
                $"Nothing new — all {allEntries.Count} loaded entries are already in the database " +
                $"({fileCount} files). Skipped {skipped} duplicates.");
            return;
        }

        await context.Spotify.AddRangeAsync(toInsert, ct);
        await context.SaveChangesAsync(ct);

        Console.WriteLine(
            $"🎉 Import completed! Files: {fileCount}, loaded: {allEntries.Count}, " +
            $"inserted: {toInsert.Count}, skipped duplicates: {skipped}");
    }

    // Identity of a single play. ts is normalised to second precision and an invariant,
    // timezone-free string so a value parsed from JSON matches the same value read back
    // from the DB regardless of DateTimeKind.
    private static string KeyOf(DateTime ts, string? trackUri, string? episodeUri, string? audiobookUri, int msPlayed)
    {
        string uri = trackUri ?? episodeUri ?? audiobookUri ?? "";
        string stamp = ts.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
        return $"{stamp}|{uri}|{msPlayed}";
    }

    public static Task ImportAsync(AppDbContext context, CancellationToken ct = default)
        => ImportAsync(context, @"C:\Users\Sefzz\Desktop\MyProject\Spotify Extended Streaming History", ct);
}

// to import Spotify data
//  dotnet run -- --import-spotify

// to enrich Spotify data
// Invoke-RestMethod -Method Post -Uri https://localhost:5000/api/spotify/enrich/backfill   ||  to backfill existing records
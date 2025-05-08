using System.Diagnostics;
using System.Text.Json;
using POCredisOM.Models;
using StackExchange.Redis;

namespace POCredisOM;

public static class ReportClassicRedisHandler
{
    private const string RedisKey = "Test_Reports";

    public static async Task GenerateAndInsertAsync(int count = 1000)
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = connection.GetDatabase();

        var reports = ReportGenerator.GenerateReports(count);
        var json = JsonSerializer.Serialize(reports);

        await db.StringSetAsync(RedisKey, json);

        Console.WriteLine($"{count} reports inserted under key '{RedisKey}' (classic Redis).");
    }

    public static async Task<List<Report>> ReadAndQueryAsync()
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = connection.GetDatabase();

        var json = await db.StringGetAsync(RedisKey);

        if (json.IsNullOrEmpty)
        {
            Console.WriteLine("No data found under key.");
            return new List<Report>();
        }

        var reports = JsonSerializer.Deserialize<List<Report>>(json!);

        // Örnek LINQ: Sadece "Open" durumundakiler
        var openReports = reports!.Where(r => r.Status == "Open").ToList();

        Console.WriteLine($"Total: {reports.Count}, Open: {openReports.Count}");
        return openReports;
    }

    public static async Task<List<Report>> QueryReportsByTitleContains(string keyword)
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = connection.GetDatabase();

        var json = await db.StringGetAsync(RedisKey);
        if (json.IsNullOrEmpty)
        {
            Console.WriteLine("No data found under key.");
            return new List<Report>();
        }

        var reports = JsonSerializer.Deserialize<List<Report>>(json!)!;

        var stopwatch = Stopwatch.StartNew();

        var filtered = reports
            .Where(r => r.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();

        stopwatch.Stop();
        Console.WriteLine(
            $"[Classic Redis] Found {filtered.Count} reports with '{keyword}' in title. Time: {stopwatch.ElapsedMilliseconds} ms"
        );

        return filtered;
    }

    public static async Task UpdateCreatedAtForReportsWithTitleAsync(string keyword)
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = connection.GetDatabase();

        var stopwatch = Stopwatch.StartNew();

        var json = await db.StringGetAsync("Test_Reports");

        if (json.IsNullOrEmpty)
        {
            Console.WriteLine("No data found.");
            return;
        }

        var reports = JsonSerializer.Deserialize<List<Report>>(json!)!;
        int updatedCount = 0;

        foreach (var report in reports)
        {
            if (report.Title.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                report.CreatedAt = DateTime.UtcNow;
                updatedCount++;
            }
        }

        var updatedJson = JsonSerializer.Serialize(reports);
        await db.StringSetAsync("Test_Reports", updatedJson);

        stopwatch.Stop();
        Console.WriteLine(
            $"[Classic Redis] Updated {updatedCount} reports with '{keyword}' in title. Time: {stopwatch.ElapsedMilliseconds} ms"
        );
    }

    public static async Task<string> GetRandomReportIdAsync()
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = connection.GetDatabase();

        var json = await db.StringGetAsync("Test_Reports");
        if (json.IsNullOrEmpty)
        {
            Console.WriteLine("[Classic Redis] No data found.");
            return string.Empty;
        }

        var reports = JsonSerializer.Deserialize<List<Report>>(json!)!;
        var random = new Random();
        return reports[random.Next(reports.Count)].Id;
    }

    public static async Task GetByIdAsync(string id)
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = connection.GetDatabase();

        var json = await db.StringGetAsync("Test_Reports");
        if (json.IsNullOrEmpty)
        {
            Console.WriteLine("[Classic Redis] No data found.");
            return;
        }

        var reports = JsonSerializer.Deserialize<List<Report>>(json!)!;

        var stopwatch = Stopwatch.StartNew();

        var report = reports.FirstOrDefault(r => r.Id == id);

        stopwatch.Stop();
        Console.WriteLine(
            $"[Classic Redis] Retrieved report by ID: {id}. Time: {stopwatch.ElapsedMilliseconds} ms"
        );
    }

    public static async Task UpdateCreatedAtByIdAsync(string id)
    {
        var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        var db = connection.GetDatabase();

        var json = await db.StringGetAsync("Test_Reports");
        if (json.IsNullOrEmpty)
        {
            Console.WriteLine("[Classic Redis] No data found.");
            return;
        }

        var reports = JsonSerializer.Deserialize<List<Report>>(json!)!;
        var stopwatch = Stopwatch.StartNew();

        var report = reports.FirstOrDefault(r => r.Id == id);
        if (report != null)
        {
            report.CreatedAt = DateTime.UtcNow;

            var updatedJson = JsonSerializer.Serialize(reports);
            await db.StringSetAsync("Test_Reports", updatedJson);
        }

        stopwatch.Stop();
        Console.WriteLine(
            $"[Classic Redis] Updated CreatedAt for ID: {id}. Time: {stopwatch.ElapsedMilliseconds} ms"
        );
    }
}

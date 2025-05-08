using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POCredisOM.Models;
using Redis.OM;

namespace POCredisOM;

public static class ReportGenerator
{
    private static readonly string[] Titles =
    [
        "Flood in",
        "Fire in",
        "Earthquake at",
        "Storm hits",
        "Incident in",
        "Outage at",
        "Evacuation in",
    ];

    private static readonly string[] Cities =
    [
        "Berlin",
        "Paris",
        "New York",
        "Tokyo",
        "Istanbul",
        "London",
        "Madrid",
        "Rome",
        "Amsterdam",
        "Vienna",
    ];

    private static readonly string[] Countries =
    [
        "Germany",
        "France",
        "USA",
        "Japan",
        "Turkey",
        "UK",
        "Spain",
        "Italy",
        "Netherlands",
        "Austria",
    ];

    private static readonly string[] Statuses = ["Open", "Closed", "InProgress"];

    private static readonly string[] TagsPool =
    [
        "weather",
        "emergency",
        "infrastructure",
        "public",
        "health",
        "transport",
        "security",
    ];

    private static readonly string[] FirstNames =
    [
        "Alice",
        "Bob",
        "Charlie",
        "Diana",
        "Ethan",
        "Fiona",
        "George",
        "Hannah",
    ];
    private static readonly string[] LastNames =
    [
        "Smith",
        "Johnson",
        "Brown",
        "Taylor",
        "Wilson",
        "Evans",
        "Clark",
    ];

    public static async Task GenerateAndInsertAsync(int count = 1000)
    {
        var provider = new RedisConnectionProvider("redis://localhost:6379");
        var redis = provider.Connection;
        var reports = provider.RedisCollection<Report>();

        try
        {
            await redis.CreateIndexAsync(typeof(Report));
        }
        catch (Exception ex) when (ex.Message.Contains("Index already exists"))
        {
            // Ignore if index already exists
        }

        var fakeReports = GenerateReports(count);

        foreach (var report in fakeReports)
        {
            await reports.InsertAsync(report);
        }

        Console.WriteLine($"{count} reports inserted into Redis.");
    }

    public static List<Report> GenerateReports(int count)
    {
        var random = new Random();
        var reports = new List<Report>();

        for (int i = 0; i < count; i++)
        {
            var cityIndex = random.Next(Cities.Length);
            var city = Cities[cityIndex];
            var country = Countries[cityIndex];
            var firstName = FirstNames[random.Next(FirstNames.Length)];
            var lastName = LastNames[random.Next(LastNames.Length)];
            var fullName = $"{firstName} {lastName}";
            var email = $"{firstName.ToLower()}.{lastName.ToLower()}@example.com";

            var report = new Report
            {
                Id = Guid.NewGuid().ToString(),
                Title = $"{Titles[random.Next(Titles.Length)]} {city}",
                Status = Statuses[random.Next(Statuses.Length)],
                CreatedAt = DateTime.UtcNow.AddMinutes(-random.Next(0, 10000)),
                Tags = GetRandomTags(random),
                Author = new Author { Name = fullName, Email = email },
                Location = new Location { Country = country, City = city },
            };

            reports.Add(report);
        }

        return reports;
    }

    private static string[] GetRandomTags(Random random)
    {
        int tagCount = random.Next(1, 4);
        var tags = new HashSet<string>();

        while (tags.Count < tagCount)
        {
            tags.Add(TagsPool[random.Next(TagsPool.Length)]);
        }

        return tags.ToArray();
    }

    public static async Task<List<Report>> QueryReportsByTitleContains(string keyword)
    {
        var provider = new RedisConnectionProvider("redis://localhost:6379");
        var reports = provider.RedisCollection<Report>();

        var stopwatch = Stopwatch.StartNew();

        var result = reports.Where(r => r.Title.Contains(keyword)).ToList();

        stopwatch.Stop();
        Console.WriteLine(
            $"[Redis OM] Found {result.Count} reports with '{keyword}' in title. Time: {stopwatch.ElapsedMilliseconds} ms"
        );

        return result;
    }

    public static void UpdateCreatedAtForReportsWithTitle(string keyword)
    {
        var provider = new RedisConnectionProvider("redis://localhost:6379");
        var reports = provider.RedisCollection<Report>();

        var stopwatch = Stopwatch.StartNew();

        var matchingReports = reports.Where(r => r.Title.Contains(keyword)).ToList();

        foreach (var report in matchingReports)
        {
            report.CreatedAt = DateTime.UtcNow;
            reports.Update(report);
        }

        stopwatch.Stop();
        Console.WriteLine(
            $"[Redis OM] Updated {matchingReports.Count} reports with '{keyword}' in title. Time: {stopwatch.ElapsedMilliseconds} ms"
        );
    }

    public static string GetRandomReportId()
    {
        var provider = new RedisConnectionProvider("redis://localhost:6379");
        var reports = provider.RedisCollection<Report>();
        var all = reports.ToList();

        if (!all.Any())
        {
            Console.WriteLine("[Redis OM] No reports found.");
            return string.Empty;
        }

        var random = new Random();
        return all[random.Next(all.Count)].Id;
    }

    public static void GetById(string id)
    {
        var provider = new RedisConnectionProvider("redis://localhost:6379");
        //var redis = provider.Connection;
        var reports = provider.RedisCollection<Report>();
        //redis.CreateIndexAsync(typeof(Report));

        var stopwatch = Stopwatch.StartNew();

        var report = reports.FirstOrDefault(r => r.Id == id);

        stopwatch.Stop();
        Console.WriteLine(
            $"[Redis OM] Retrieved report by ID: {id}. Time: {stopwatch.ElapsedMilliseconds} ms"
        );
    }

    public static void UpdateCreatedAtById(string id)
    {
        var provider = new RedisConnectionProvider("redis://localhost:6379");
        var reports = provider.RedisCollection<Report>();

        var stopwatch = Stopwatch.StartNew();

        var report = reports.FirstOrDefault(r => r.Id == id);
        if (report != null)
        {
            report.CreatedAt = DateTime.UtcNow;
            reports.Update(report);
        }

        stopwatch.Stop();
        Console.WriteLine(
            $"[Redis OM] Updated CreatedAt for ID: {id}. Time: {stopwatch.ElapsedMilliseconds} ms"
        );
    }

    public static async Task<List<Report>> QueryReportsByCity(string city)
    {
        var provider = new RedisConnectionProvider("redis://localhost:6379");
        var reports = provider.RedisCollection<Report>();

        var stopwatch = Stopwatch.StartNew();

        var result = reports.Where(r => r.Location.City == city).ToList();

        stopwatch.Stop();
        Console.WriteLine(
            $"[Redis OM] Found {result.Count} reports in city '{city}'. Time: {stopwatch.ElapsedMilliseconds} ms"
        );

        return result;
    }
}

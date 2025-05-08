using System.Diagnostics;
using POCredisOM;
using POCredisOM.Models;
using Redis.OM;
using Redis.OM.Searching;
using StackExchange.Redis;

var provider = new RedisConnectionProvider("redis://localhost:6379");
var redis = provider.Connection;
var reports = provider.RedisCollection<Report>();

//await WipeAllReportKeysAsync();

// Create Reports for testing. Comment below line if you don't want to generate new reports.
//await ReportGenerator.GenerateAndInsertAsync(500000);

// Classic Redis way to generate and insert reports
//await ReportClassicRedisHandler.GenerateAndInsertAsync(500000);
Console.WriteLine("Querying for reports with 'Berlin' in Title...\n");

// Redis OM - Query
LogMemoryUsage("Before Redis OM Query");
await ReportGenerator.QueryReportsByTitleContains("Berlin");
LogMemoryUsage("After Redis OM Query\n");

// Classic Redis - Query
LogMemoryUsage("Before Classic Redis Query");
await ReportClassicRedisHandler.QueryReportsByTitleContains("Berlin");
LogMemoryUsage("After Classic Redis Query\n");

// Redis OM - Update
LogMemoryUsage("Before Redis OM Update");
ReportGenerator.UpdateCreatedAtForReportsWithTitle("Berlin");
LogMemoryUsage("After Redis OM Update\n");

// Classic Redis - Update
LogMemoryUsage("Before Classic Redis Update");
await ReportClassicRedisHandler.UpdateCreatedAtForReportsWithTitleAsync("Berlin");
LogMemoryUsage("After Classic Redis Update\n");

Console.WriteLine("Fetching random IDs...\n");

string redisOmId = ReportGenerator.GetRandomReportId();
string classicId = await ReportClassicRedisHandler.GetRandomReportIdAsync();

Console.WriteLine($"[Redis OM] Random ID: {redisOmId}");
Console.WriteLine($"[Classic Redis] Random ID: {classicId}\n");

// Redis OM Get by ID
LogMemoryUsage("Before Redis OM GetById");
ReportGenerator.GetById(redisOmId);
LogMemoryUsage("After Redis OM GetById\n");

// Classic Redis Get by ID
LogMemoryUsage("Before Classic Redis GetById");
await ReportClassicRedisHandler.GetByIdAsync(classicId);
LogMemoryUsage("After Classic Redis GetById\n");

Console.WriteLine("\nUpdating CreatedAt by ID...\n");

LogMemoryUsage("Before Redis OM ID Update");
ReportGenerator.UpdateCreatedAtById(redisOmId);
LogMemoryUsage("After Redis OM ID Update\n");

LogMemoryUsage("Before Classic Redis ID Update");
await ReportClassicRedisHandler.UpdateCreatedAtByIdAsync(classicId);
LogMemoryUsage("After Classic Redis ID Update\n");

static void LogMemoryUsage(string label)
{
    long managedMemory = GC.GetTotalMemory(false);
    long privateMemory = Process.GetCurrentProcess().PrivateMemorySize64;

    Console.WriteLine(
        $"[{label}] Managed: {managedMemory / 1024.0 / 1024.0:F2} MB | Private: {privateMemory / 1024.0 / 1024.0:F2} MB"
    );
}

static async Task WipeAllReportKeysAsync()
{
    var connection = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
    var db = connection.GetDatabase();
    var server = connection.GetServer("localhost", 6379);

    // Report:* key’lerini sil
    var reportKeys = server.Keys(pattern: "Report:*").ToArray();
    foreach (var key in reportKeys)
    {
        await db.KeyDeleteAsync(key);
    }

    // Classic Redis key’ini sil
    await db.KeyDeleteAsync("Test_Reports");

    Console.WriteLine($"Deleted {reportKeys.Length} Report:* keys and Test_Reports.");
}

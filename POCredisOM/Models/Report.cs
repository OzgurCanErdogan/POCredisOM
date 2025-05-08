using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POCredisOM.Models;

using Redis.OM.Modeling;

[Document(StorageType = StorageType.Json)]
public class Report
{
    [RedisIdField]
    [Indexed]
    public string Id { get; set; }

    [Indexed]
    public string Title { get; set; }

    [Indexed]
    public string Status { get; set; } // e.g., "Open", "Closed", "InProgress"

    [Indexed]
    public DateTime CreatedAt { get; set; }

    public Author Author { get; set; }

    [Indexed]
    public string[] Tags { get; set; }

    [Indexed]
    public Location Location { get; set; }
}

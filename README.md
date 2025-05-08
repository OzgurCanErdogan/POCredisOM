## 🔍 Redis OM vs Classic Redis Benchmark – Report Caching POC

This proof of concept compares **Redis OM** and **Classic Redis** for storing and querying `Report` objects at scale. Tests were performed using 100K and 500K item datasets with the following operations:

### ✅ Scenarios Tested

* Querying by title (`WHERE Title CONTAINS 'Berlin'`)
* Updating `CreatedAt` for matching reports
* `GetById` and `UpdateById` by `Id`
* Memory usage (managed + private)
* Performance under 500K record load

---

### 📊 Results Summary (500,000 Reports)

| Operation                 | Redis OM                          | Classic Redis                       |
| ------------------------- | --------------------------------- | ----------------------------------- |
| **Query (Title filter)**  | \~2.0 sec (RediSearch)            | \~29 ms (LINQ on deserialized JSON) |
| **Update (Batch)**        | \~9.3 sec (per-record `JSON.SET`) | \~3.8 sec (full overwrite)          |
| **GetById**               | \~3 ms                            | \~5 ms                              |
| **UpdateById**            | \~2 ms                            | \~1.3 sec                           |
| **Memory (after update)** | \~1 GB total                      | \~1.2 GB total                      |

---

### 🧠 Key Observations

* Redis OM is ideal for structured access patterns (LINQ-like queries) and uses less memory overall.
* Classic Redis is much faster for bulk reads and simple updates but scales poorly in memory usage.
* `UpdateById` is **slower in Classic Redis** due to full serialization overhead.

---

### ⚠️ Limitation with Nested Fields

> Even though we decorated `Location.City` with `[Indexed]`, **Redis OM (and RediSearch 2.x)** currently does **not support indexing nested JSON fields**.

As a result, LINQ queries like `Where(r => r.Location.City == "Berlin")` will **not work**.
To query by nested values, those properties must be **flattened** into top-level fields.

---

(Note: To run this you need a docker and run the following to use redis docker run -d -p 6379:6379 -p 4001:8001 --name redis-stack redis/redis-stack:latest)

### 🧪 Additional Run – 500K Reports (After System Idle)

A follow-up benchmark was run after allowing the system to rest, confirming consistency of previous observations.

**Key Metrics:**

| Operation            | Redis OM            | Classic Redis       |
| -------------------- | ------------------- | ------------------- |
| Query (Title filter) | 1.97 sec            | 28 ms               |
| Batch Update         | 8.9 sec (10K items) | 4.7 sec (50K items) |
| GetById              | 3 ms                | 1 ms                |
| UpdateById           | 2 ms                | 1.5 sec             |
| Memory Peak          | \~931 MB            | \~1200 MB           |


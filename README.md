# MemoryCacheManager
<b>Memory Cache Manager</b> is a multi-thread safe caching mechanims

## Nuget package
There is a nuget package avaliable here .

# Examples

## Add and Get simple cache

```csharp
const string cacheKey = "MyCache";
const int value = 123;

// Add the cache
Cache.AddCache(cacheKey, value);

// Get the cache
var cacheExists = Cache.TryGetCache(cacheKey, out int cachedValue);
// cacheValue: 123
```

## Add and Get cache with parameters

```csharp
const string cacheKey = "UserId";
const user1 = 'user1';
const user1Id = 1;
const user2 = 'user2';
const user2Id = 2;

// Add the cache
Cache.AddCache(cacheKey, user1Id, user1);
Cache.AddCache(cacheKey, user2Id, user2);

// Get the cache
Cache.TryGetCache(cacheKey, out int userId1Cached, user1);
Cache.TryGetCache(cacheKey, out int userId2Cached, user2);
// userId1Cached: 1
// userId2Cached: 2
```

## Add and Get cache with expiration time

```csharp
const string cacheKey = "MyCache";
const int value = 123;
var expirationTime = new TimeSpan(0, 10, 0); // Keep the cache per 10 minutes

// Add the cache
Cache.AddCacheExpirationTime(cacheKey, value, expirationTime);

// Get the cache
Cache.TryGetCache(cacheKey, out int cachedValue);
// cacheValue: 123
```

## Runner method

In this case, Memory cache will try to find a cache value, if there is no cache by user name, it will process the block.

```csharp
public int GetUserId(string userName)
{
    return Cache.GetCacheOrRunAndCacheWithExpirationTime("UserCache", new TimeSpan(0, 1, 0), () =>
    {
        // This block will run only if there is no cached value
        // Get User ID by User Name...
        var userId = new Random().Next();
        return userId;
    }, userName);
}
```

## License

MIT

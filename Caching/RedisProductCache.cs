using System.Text.Json;
using StackExchange.Redis;

public class RedisProductCache : IProductCache
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromSeconds(30);

    private readonly IConnectionMultiplexer _redis;

    public RedisProductCache(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<Product?> GetAsync(int productId)
    {
        var database = _redis.GetDatabase();

        var value = await database.StringGetAsync(GetKey(productId));

        if (!value.HasValue)
        {
            return null;
        }

        var json = value.ToString();

        return JsonSerializer.Deserialize<Product>(json);
    }

    public async Task SetAsync(Product product)
    {
        var database = _redis.GetDatabase();

        var json = JsonSerializer.Serialize(product);

        await database.StringSetAsync(
            GetKey(product.Id),
            json,
            CacheExpiration
        );
    }

    public async Task RemoveAsync(int productId)
    {
        var database = _redis.GetDatabase();

        await database.KeyDeleteAsync(GetKey(productId));
    }

    private static string GetKey(int productId)
    {
        return $"product:{productId}";
    }
}
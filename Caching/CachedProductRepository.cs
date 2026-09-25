using Microsoft.Extensions.Logging;

public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _innerRepository;
    private readonly IProductCache _cache;
    private readonly ILogger<CachedProductRepository> _logger;

    public CachedProductRepository(
        IProductRepository innerRepository,
        IProductCache cache,
        ILogger<CachedProductRepository> logger)
    {
        _innerRepository = innerRepository;
        _cache = cache;
        _logger = logger;
    }

    public Task<List<Product>> GetAllAsync()
    {
        return _innerRepository.GetAllAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        var cachedProduct = await _cache.GetAsync(id);

        if (cachedProduct is not null)
        {
            _logger.LogInformation("Redis cache hit for product {ProductId}", id);

            return cachedProduct;
        }

        _logger.LogInformation("Redis cache miss for product {ProductId}", id);

        var product = await _innerRepository.GetByIdAsync(id);

        if (product is not null)
        {
            await _cache.SetAsync(product);
        }

        return product;
    }

    public async Task AddAsync(Product product)
    {
        await _innerRepository.AddAsync(product);
    }

    public async Task UpdateAsync(Product product)
    {
        await _innerRepository.UpdateAsync(product);

        await _cache.RemoveAsync(product.Id);
    }

    public async Task DeleteAsync(Product product)
    {
        await _innerRepository.DeleteAsync(product);

        await _cache.RemoveAsync(product.Id);
    }
}
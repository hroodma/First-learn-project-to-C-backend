using Microsoft.Extensions.Logging;

public class EventingProductService : IProductService
{
    private readonly IProductService _inner;
    private readonly IProductEventPublisher _publisher;
    private readonly ILogger<EventingProductService> _logger;

    public EventingProductService(
        IProductService inner,
        IProductEventPublisher publisher,
        ILogger<EventingProductService> logger)
    {
        _inner = inner;
        _publisher = publisher;
        _logger = logger;
    }

    public Task<List<ProductDto>> GetAllAsync()
    {
        return _inner.GetAllAsync();
    }

    public Task<ProductDto?> GetByIdAsync(int id)
    {
        return _inner.GetByIdAsync(id);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var product = await _inner.CreateAsync(request);

        _logger.LogInformation(
            "Публикуем событие ProductCreated для товара {ProductId}",
            product.Id
        );

        await _publisher.PublishProductCreatedAsync(product);

        return product;
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request)
    {
        var product = await _inner.UpdateAsync(id, request);

        if (product is not null)
        {
            _logger.LogInformation(
                "Публикуем событие ProductUpdated для товара {ProductId}",
                product.Id
            );

            await _publisher.PublishProductUpdatedAsync(product);
        }

        return product;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _inner.DeleteAsync(id);

        if (deleted)
        {
            _logger.LogInformation(
                "Публикуем событие ProductDeleted для товара {ProductId}",
                id
            );

            await _publisher.PublishProductDeletedAsync(id);
        }

        return deleted;
    }
}
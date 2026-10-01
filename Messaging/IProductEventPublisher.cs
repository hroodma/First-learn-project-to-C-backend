public interface IProductEventPublisher
{
    Task PublishProductCreatedAsync(ProductDto product);

    Task PublishProductUpdatedAsync(ProductDto product);

    Task PublishProductDeletedAsync(int productId);
}
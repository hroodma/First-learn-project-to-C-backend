public interface IProductCache
{
    Task<Product?> GetAsync(int productId);

    Task SetAsync(Product product);

    Task RemoveAsync(int productId);
}
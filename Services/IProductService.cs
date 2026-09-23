public interface IProductService
{
    Task<List<ProductDto>> GetAllAsync();

    Task<ProductDto?> GetByIdAsync(int id);

    Task<ProductDto> CreateAsync(CreateProductRequest request);

    Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request);

    Task<bool> DeleteAsync(int id);
}
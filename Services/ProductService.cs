using Microsoft.Extensions.Logging;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();

        return products
            .Select(MapToDto)
            .ToList();
    }

    public async Task<ProductDto?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return null;
        }

        return MapToDto(product);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        Validate(request.Name, request.Price);

        var product = new Product
        {
            Name = request.Name.Trim(),
            Price = request.Price
        };

        await _productRepository.AddAsync(product);

        _logger.LogInformation("Создан товар с id {ProductId}", product.Id);

        return MapToDto(product);
    }

    public async Task<ProductDto?> UpdateAsync(int id, UpdateProductRequest request)
    {
        Validate(request.Name, request.Price);

        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return null;
        }

        product.Name = request.Name.Trim();
        product.Price = request.Price;

        await _productRepository.UpdateAsync(product);

        _logger.LogInformation("Обновлен товар с id {ProductId}", product.Id);

        return MapToDto(product);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);

        if (product is null)
        {
            return false;
        }

        await _productRepository.DeleteAsync(product);

        _logger.LogInformation("Удален товар с id {ProductId}", product.Id);

        return true;
    }

    private static void Validate(string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название товара не может быть пустым");
        }

        if (price < 0)
        {
            throw new ArgumentException("Цена товара не может быть отрицательной");
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto(
            product.Id,
            product.Name,
            product.Price
        );
    }
}
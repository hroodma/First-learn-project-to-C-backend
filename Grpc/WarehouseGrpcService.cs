using System.Globalization;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using WarehouseApi.Grpc;

public class WarehouseGrpcService : Warehouse.WarehouseBase
{
    private readonly IProductService _productService;
    private readonly ILogger<WarehouseGrpcService> _logger;

    public WarehouseGrpcService(
        IProductService productService,
        ILogger<WarehouseGrpcService> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public override async Task<ProductResponse> GetProduct(
        GetProductRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation("gRPC GetProduct запрос для товара {ProductId}", request.Id);

        if (request.Id <= 0)
        {
            throw new RpcException(new Status(
                StatusCode.InvalidArgument,
                "Id товара должен быть больше нуля"
            ));
        }

        var product = await _productService.GetByIdAsync(request.Id);

        if (product is null)
        {
            throw new RpcException(new Status(
                StatusCode.NotFound,
                $"Товар с id {request.Id} не найден"
            ));
        }

        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price.ToString(CultureInfo.InvariantCulture)
        };
    }
}
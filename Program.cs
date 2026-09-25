using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Строка подключения 'DefaultConnection' не найдена в appsettings.json"
    );
}

builder.Services.AddDbContext<WarehouseDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

// Redis
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")
    ?? "localhost:6379";

builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    return ConnectionMultiplexer.Connect(redisConnectionString);
});

builder.Services.AddSingleton<IProductCache, RedisProductCache>();

// Архитектура
builder.Services.AddScoped<ProductRepository>();

builder.Services.AddScoped<IProductRepository>(serviceProvider =>
{
    return new CachedProductRepository(
        serviceProvider.GetRequiredService<ProductRepository>(),
        serviceProvider.GetRequiredService<IProductCache>(),
        serviceProvider.GetRequiredService<ILogger<CachedProductRepository>>()
    );
});

builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse API v1");
});

app.MapGet("/", () => "Warehouse API is running. Open /swagger");

// Получить все товары
app.MapGet("/api/products", async (IProductService productService) =>
{
    var products = await productService.GetAllAsync();

    return Results.Ok(products);
});

// Получить один товар
app.MapGet("/api/products/{id:int}", async (int id, IProductService productService) =>
{
    var product = await productService.GetByIdAsync(id);

    if (product is null)
    {
        return Results.NotFound(new
        {
            message = $"Товар с id {id} не найден"
        });
    }

    return Results.Ok(product);
});

// Создать товар
app.MapPost("/api/products", async (CreateProductRequest request, IProductService productService) =>
{
    try
    {
        var product = await productService.CreateAsync(request);

        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new
        {
            message = ex.Message
        });
    }
});

// Обновить товар
app.MapPut("/api/products/{id:int}", async (int id, UpdateProductRequest request, IProductService productService) =>
{
    try
    {
        var product = await productService.UpdateAsync(id, request);

        if (product is null)
        {
            return Results.NotFound(new
            {
                message = $"Товар с id {id} не найден"
            });
        }

        return Results.Ok(product);
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new
        {
            message = ex.Message
        });
    }
});

// Удалить товар
app.MapDelete("/api/products/{id:int}", async (int id, IProductService productService) =>
{
    var deleted = await productService.DeleteAsync(id);

    if (!deleted)
    {
        return Results.NotFound(new
        {
            message = $"Товар с id {id} не найден"
        });
    }

    return Results.NoContent();
});

app.Run("http://localhost:5000");
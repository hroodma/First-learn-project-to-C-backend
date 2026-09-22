using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Подключаем базу данных
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

var app = builder.Build();

// Swagger UI
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse API v1");
});

app.MapGet("/", () => "Warehouse API is running. Open /swagger");

// Получить все товары
app.MapGet("/api/products", async (WarehouseDbContext dbContext) =>
{
    var products = await dbContext.Products
        .AsNoTracking()
        .OrderBy(p => p.Id)
        .ToListAsync();

    return Results.Ok(products);
});

// Получить один товар
app.MapGet("/api/products/{id:int}", async (int id, WarehouseDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(id);

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
app.MapPost("/api/products", async (CreateProductRequest request, WarehouseDbContext dbContext) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new
        {
            message = "Название товара не может быть пустым"
        });
    }

    if (request.Price < 0)
    {
        return Results.BadRequest(new
        {
            message = "Цена товара не может быть отрицательной"
        });
    }

    var product = new Product
    {
        Name = request.Name.Trim(),
        Price = request.Price
    };

    dbContext.Products.Add(product);

    await dbContext.SaveChangesAsync();

    return Results.Created($"/api/products/{product.Id}", product);
});

// Обновить товар
app.MapPut("/api/products/{id:int}", async (int id, CreateProductRequest request, WarehouseDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(id);

    if (product is null)
    {
        return Results.NotFound(new
        {
            message = $"Товар с id {id} не найден"
        });
    }

    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new
        {
            message = "Название товара не может быть пустым"
        });
    }

    if (request.Price < 0)
    {
        return Results.BadRequest(new
        {
            message = "Цена товара не может быть отрицательной"
        });
    }

    product.Name = request.Name.Trim();
    product.Price = request.Price;

    await dbContext.SaveChangesAsync();

    return Results.Ok(product);
});

// Удалить товар
app.MapDelete("/api/products/{id:int}", async (int id, WarehouseDbContext dbContext) =>
{
    var product = await dbContext.Products.FindAsync(id);

    if (product is null)
    {
        return Results.NotFound(new
        {
            message = $"Товар с id {id} не найден"
        });
    }

    dbContext.Products.Remove(product);

    await dbContext.SaveChangesAsync();

    return Results.NoContent();
});

app.Run("http://localhost:5000");
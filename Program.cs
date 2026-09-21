using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Говорок: нам нужен Swagger.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Включаем Swagger.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Warehouse API v1");
});

// Главная страница, чтобы быстро проверить, что сервер работает.
app.MapGet("/", () => "Warehouse API is running. Open /swagger");

// Временное хранилище товаров в памяти.
var products = new List<Product>();

// Получить все товары.
app.MapGet("/api/products", () =>
{
    return Results.Ok(products);
});

// Получить один товар по id.
app.MapGet("/api/product/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);

    if (product is null)
    {
        return Results.NotFound(new
        {
            message = $"Товар с id {id} не найден"
        });
    }

    return Results.Ok(product);
});

// Создать новый товар.
app.MapPost("/api/products", (CreateProductRequest request) =>
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

    var nextId = products.Count == 0
        ? 1
        : products.Max(p => p.Id) + 1;

    var product = new Product(
        nextId,
        request.Name.Trim(),
        request.Price
    );

    products.Add(product);

    return Results.Created($"/api/products/{product.Id}", product);
});

// Обновить товар по id.
app.MapPut("/api/products/{id:int}", (int id, CreateProductRequest request) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);

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

    var updatedProduct = product with
    {
        Name = request.Name.Trim(),
        Price = request.Price
    };

    var index = products.IndexOf(product);
    products[index] = updatedProduct;

    return Results.Ok(updatedProduct);
});

// Удалить товар по id.
app.MapDelete("/api/products/{id:int}", (int id) =>
{
    var product = products.FirstOrDefault(p => p.Id == id);

    if (product is null)
    {
        return Results.NotFound(new
        {
            message = $"Товар с id {id} не найден"
        });
    }

    products.Remove(product);

    return Results.NoContent();
});

// Запускаем сервер на порту 5000.
app.Run("http://localhost:5000");

// Модель товара.
record Product(int Id, string Name, decimal Price);

// Модель для запроса на создание/обновление товара.
record CreateProductRequest(string Name, decimal Price);
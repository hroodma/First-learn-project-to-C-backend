using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

public class KafkaProductEventPublisher : IProductEventPublisher, IDisposable
{
    private readonly IProducer<Null, string> _producer;
    private readonly string _topic;
    private readonly ILogger<KafkaProductEventPublisher> _logger;

    public KafkaProductEventPublisher(
        IConfiguration configuration,
        ILogger<KafkaProductEventPublisher> logger)
    {
        _logger = logger;

        var bootstrapServers = configuration.GetValue<string>("Kafka:BootstrapServers")
            ?? "localhost:9092";

        _topic = configuration.GetValue<string>("Kafka:Topic")
            ?? "warehouse.product-events";

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.Leader
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public Task PublishProductCreatedAsync(ProductDto product)
    {
        return PublishAsync(
            "ProductCreated",
            product.Id,
            product.Name,
            product.Price
        );
    }

    public Task PublishProductUpdatedAsync(ProductDto product)
    {
        return PublishAsync(
            "ProductUpdated",
            product.Id,
            product.Name,
            product.Price
        );
    }

    public Task PublishProductDeletedAsync(int productId)
    {
        return PublishAsync(
            "ProductDeleted",
            productId,
            null,
            null
        );
    }

    private async Task PublishAsync(
        string eventType,
        int productId,
        string? name,
        decimal? price)
    {
        var productEvent = new ProductEvent
        {
            EventType = eventType,
            ProductId = productId,
            Name = name,
            Price = price,
            OccurredAtUtc = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(productEvent);

        var message = new Message<Null, string>
        {
            Value = json
        };

        try
        {
            var deliveryResult = await _producer.ProduceAsync(_topic, message);

            _logger.LogInformation(
                "Kafka: опубликовано событие {EventType} для товара {ProductId}. Partition: {Partition}, Offset: {Offset}",
                eventType,
                productId,
                deliveryResult.Partition.Value,
                deliveryResult.Offset.Value
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Kafka: не удалось опубликовать событие {EventType} для товара {ProductId}",
                eventType,
                productId
            );
        }
    }

    public void Dispose()
    {
        _producer.Dispose();
    }
}
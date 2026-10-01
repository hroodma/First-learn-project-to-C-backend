public class ProductEvent
{
    public string EventType { get; set; } = string.Empty;

    public int ProductId { get; set; }

    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public DateTime OccurredAtUtc { get; set; }
}
using System.Text.Json;

namespace App.SharedKernel.Outbox;

public class OutboxMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string EventType { get; set; } = string.Empty;

    public string EventData { get; set; } = string.Empty;

    public bool IsProcessed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }

    public static OutboxMessage Create<T>(T eventData)
    {
        return new OutboxMessage
        {
            EventType = typeof(T).Name,
            EventData = JsonSerializer.Serialize(eventData),
            CreatedAt = DateTime.UtcNow
        };
    }

    public T? Deserialize<T>()
    {
        return JsonSerializer.Deserialize<T>(EventData);
    }
}

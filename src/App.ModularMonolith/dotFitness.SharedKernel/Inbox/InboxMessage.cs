using System;

namespace dotFitness.SharedKernel.Inbox;

public class InboxMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    public string EventId { get; set; } = string.Empty;

    public string EventType { get; set; } = string.Empty;

    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;

    public string Payload { get; set; } = string.Empty;

    public string Consumer { get; set; } = string.Empty;

    public string Status { get; set; } = "pending"; // pending|processed|failed

    public DateTime? ProcessedAt { get; set; }

    public int Attempts { get; set; } = 0;

    public string? Error { get; set; }

    public string? CorrelationId { get; set; }

    public string? TraceId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}




using Dapper.Contrib.Extensions;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

[Table("AccountEvents")]
internal record AccountEventEntity
{
    /// <summary>
    /// Global, gapless ordering of all events.
    /// </summary>    
    [Write(false)]
    public long SequenceId { get; set; }

    /// <summary>
    /// Unique event identifier for idempotency.
    /// </summary>
    public Guid EventId { get; set; }

    /// <summary>
    /// Aggregate key (email-based AccountId).
    /// </summary>
    public string AccountId { get; set; } = default!;

    /// <summary>
    /// Per-aggregate, gapless version for concurrency.
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Domain event type (e.g. 'MoneyDeposited').
    /// </summary>
    public string EventType { get; set; } = default!;

    /// <summary>
    /// JSON payload containing event data.
    /// </summary>
    public string Data { get; set; } = default!;

    /// <summary>
    /// Timestamp when the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Tracks payload schema version for evolution.
    /// </summary>
    public int SchemaVersion { get; set; }
}

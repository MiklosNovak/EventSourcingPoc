using System.ComponentModel.DataAnnotations.Schema;

namespace BankAccount.Writer.Repositories.OutboxEvents;

[Table("OutboxEvents")]
public class OutboxEventEntity
{
    public int Version { get; set; }

    public string EventType { get; set; }

    public string Data { get; set; }

    public bool Published { get; set; }
}

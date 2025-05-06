using Dapper.Contrib.Extensions;
using TableAttribute = System.ComponentModel.DataAnnotations.Schema.TableAttribute;

namespace BankAccount.Writer.Repositories.OutboxEvents;

[Table("OutboxEvents")]
public class OutboxEventEntity
{
    [Write(false)]
    public long SequenceId { get; set; }

    public int Version { get; set; }

    public string EventType { get; set; }

    public string Data { get; set; }

    public bool Published { get; set; }
}

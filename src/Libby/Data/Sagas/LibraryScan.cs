using System.ComponentModel.DataAnnotations;
using MassTransit;

namespace Libby.Data.Sagas;

public sealed class LibraryScan : SagaStateMachineInstance
{
    public required Guid CorrelationId { get; set; }

    public required string CurrentState { get; set; }

    public List<Guid>? ItemsImporting { get; set; }

    public List<Guid>? ItemsImported { get; set; }

    public DateTimeOffset? StatusDate { get; set; }

    [Timestamp]
    public uint Version { get; set; }
}

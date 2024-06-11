namespace Libby.Contracts;

public sealed class LibraryItemImported
{
    public required Guid CorrelationId { get; init; }

    public required Guid LibraryId { get; init; }

    public required Guid LibraryItemId { get; init; }
}

namespace Libby.Contracts;

public sealed class ImportLibraryItem
{
    public required Guid CorrelationId { get; init; }
    public required Guid LibraryId { get; init; }
    public required string FilePath { get; init; }
}

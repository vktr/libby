namespace Libby.Contracts;

public sealed class LibraryCreated
{
    public required Guid LibraryId { get; init; }
    public required string LibraryPath { get; init; }
}

public sealed class GetFiles
{
    public required Guid CorrelationId { get; init; }
    public required string Path { get; init; }
    public required IList<string> IncludePatterns { get; init; }
}

public sealed class GetFilesResult
{
    public required Guid CorrelationId { get; init; }
    public required IList<string> FilePaths { get; init; }
}

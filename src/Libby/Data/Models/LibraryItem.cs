using System.Text.Json;

namespace Libby.Data.Models;

public sealed class LibraryItem : IDisposable
{
    public required Guid Id { get; init; }

    public required Library Library { get; init; }

    public required string FileName { get; init; }

    public required long FileSize { get; init; }

    public JsonDocument? FfprobeJson { get; set; }

    public string? FFprobeError { get; set; }

    public DateTimeOffset? FfprobeDate { get; set; }

    public void Dispose() => FfprobeJson?.Dispose();
}

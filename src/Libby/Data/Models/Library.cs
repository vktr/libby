using System.ComponentModel.DataAnnotations;

namespace Libby.Data.Models;

public sealed class Library
{
    public required Guid Id { get; init; }

    [MaxLength(250)]
    public required string Name { get; init; }

    [MaxLength(250)]
    public required string Path { get; init; }
}
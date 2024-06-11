using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Libby.Contracts;

public sealed class CreateLibrary
{
    [JsonPropertyName("name")]
    [Required]
    [MaxLength(250)]
    public required string Name { get; init; }

    [JsonPropertyName("path")]
    [Required]
    [MaxLength(250)]
    public required string Path { get; init; }
}
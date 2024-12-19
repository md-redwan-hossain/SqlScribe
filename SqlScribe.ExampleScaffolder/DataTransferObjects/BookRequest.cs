using System.ComponentModel.DataAnnotations;

namespace SqlScribe.ExampleScaffolder.DataTransferObjects;

public record BookRequest
{
    [MaxLength(256)] public required string Title { get; init; }
    [MaxLength(38)] public required string Isbn { get; init; }

    [MaxLength(256)] public required string Genre { get; init; }

    [MaxLength(256)] public required int? AuthorId { get; init; }

    public required decimal Price { get; init; }
}
namespace SqlScribe.ExampleScaffolder.DataTransferObjects;

public record BookResponse
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Isbn { get; init; }
    public required string Genre { get; init; }
    public required string? AuthorFirstName { get; set; }
    public required string AuthorLastName { get; set; }
    public required decimal Price { get; init; }
}
namespace SqlScribe.ExampleScaffolder.Domain;

public class Book
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required string Isbn { get; set; }
    public required string Genre { get; set; }
    public required int? AuthorId { get; set; }
    public required decimal Price { get; set; }
}
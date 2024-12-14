namespace SqlScribe.HttpApiExample.Data;

public class Book
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Isbn { get; set; }
    public required string Genre { get; set; }
    public required string Author { get; set; }
    public required decimal Price { get; set; }
}
namespace SqlScribe.ExampleScaffolder.DataTransferObjects;

public record BookSlimResponse
{
    public required string Title { get; init; }
    public required decimal Price { get; init; }
}
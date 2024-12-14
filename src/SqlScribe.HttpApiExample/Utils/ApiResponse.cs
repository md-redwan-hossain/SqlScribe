namespace SqlScribe.HttpApiExample.Utils;

public readonly record struct ApiResponse
{
    public required bool Success { get; init; }
    public required int Code { get; init; }
    public required string Message { get; init; }
    public object? Data { get; init; }
}
using System.ComponentModel.DataAnnotations;

namespace SqlScribe.HttpApiExample.DataTransferObjects;

public record BookRequest(
    [MaxLength(256)] [Required] string Title,
    [MaxLength(38)] [Required] string Isbn,
    [MaxLength(256)] [Required] string Genre,
    [MaxLength(256)] [Required] string Author,
    [Range(1, int.MaxValue)] [Required] int Price
);
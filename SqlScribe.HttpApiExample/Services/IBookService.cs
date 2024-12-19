using SharpOutcome;
using SharpOutcome.Helpers.Contracts;
using SqlScribe.ExampleScaffolder.DataTransferObjects;
using SqlScribe.ExampleScaffolder.Domain;

namespace SqlScribe.HttpApiExample.Services;

public interface IBookService
{
    Task<ValueOutcome<Book, IBadOutcome>> CreateAsync(BookRequest dto);
    Task<ValueOutcome<Book, IBadOutcome>> UpdateAsync(int id, BookRequest dto);
    Task<IEnumerable<BookResponse>> GetAllAsync();
    Task<IEnumerable<BookSlimResponse>> GetAllByPriceRangeAsync(decimal lowerBound, decimal upperBound,
        CancellationToken ct);
    Task<ValueOutcome<BookResponse, IBadOutcome>> GetOneAsync(int id);
    Task<ValueOutcome<IGoodOutcome, IBadOutcome>> RemoveAsync(int id);
}
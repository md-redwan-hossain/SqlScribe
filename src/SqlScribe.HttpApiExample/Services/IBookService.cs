using SharpOutcome;
using SharpOutcome.Helpers.Contracts;
using SqlScribe.ExampleScaffolder.Domain;
using SqlScribe.HttpApiExample.DataTransferObjects;

namespace SqlScribe.HttpApiExample.Services;

public interface IBookService
{
    Task<ValueOutcome<Book, IBadOutcome>> CreateAsync(BookRequest dto);
    Task<ValueOutcome<Book, IBadOutcome>> UpdateAsync(int id, BookRequest dto);
    Task<IEnumerable<Book>> GetAllAsync();

    Task<IEnumerable<BookSlimResponse>> GetAllByPriceRangeAsync(decimal lowerBound, decimal upperBound,
        CancellationToken ct);
    Task<ValueOutcome<Book, IBadOutcome>> GetOneAsync(int id);
    Task<ValueOutcome<IGoodOutcome, IBadOutcome>> RemoveAsync(int id);
}
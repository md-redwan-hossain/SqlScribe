using SharpOutcome;
using SharpOutcome.Helpers.Contracts;
using SqlScribe.HttpApiExample.Data;
using SqlScribe.HttpApiExample.DataTransferObjects;

namespace SqlScribe.HttpApiExample.Services;

public interface IBookService
{
    Task<ValueOutcome<Book, IBadOutcome>> CreateAsync(BookRequest dto);
    Task<ValueOutcome<Book, IBadOutcome>> UpdateAsync(int id, BookRequest dto);
    Task<IList<Book>> GetAllAsync();
    Task<IList<BookSlimResponse>> GetAllByMinimumPriceAsync(decimal price, CancellationToken ct);
    Task<ValueOutcome<Book, IBadOutcome>> GetOneAsync(int id);
    Task<ValueOutcome<IGoodOutcome, IBadOutcome>> RemoveAsync(int id);
}
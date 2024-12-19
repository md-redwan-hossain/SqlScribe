using System.Security.Cryptography;
using SqlScribe.ExampleScaffolder.Domain;

namespace SqlScribe.ExampleScaffolder.Persistence;

public static class DataSeeder
{
    public static List<Author> GenerateAuthorData(int amount = 1000)
    {
        return new Bogus.Faker<Author>()
            .RuleFor(b => b.Id, f => f.IndexFaker + 1)
            .RuleFor(b => b.FirstName, f => f.Lorem.Word())
            .RuleFor(b => b.LastName, f => f.Lorem.Word())
            .Generate(amount);
    }

    public static List<Book> GenerateBookData(List<Author> authors, int amount = 1000)
    {
        List<string> genres = ["Fantasy", "Sci-Fi", "Mystery", "Romance", "Horror", "Non-fiction"];

        return new Bogus.Faker<Book>()
            .RuleFor(b => b.Id, f => f.IndexFaker + 1)
            .RuleFor(b => b.Isbn, _ => Guid.NewGuid().ToString())
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Genre, f => f.PickRandom(genres))
            .RuleFor(b => b.AuthorId,
                _ => authors.FirstOrDefault(x => x.Id == RandomNumberGenerator.GetInt32(1, amount + 1))?.Id)
            .RuleFor(b => b.Price, f => f.Random.Int(100, 1000))
            .Generate(amount);
    }
}
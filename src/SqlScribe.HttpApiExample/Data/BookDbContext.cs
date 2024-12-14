using System.Reflection;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace SqlScribe.HttpApiExample.Data;

public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.Entity<Book>().HasData(GenerateData());
        
        
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.GetTableName() == "__EFMigrationsHistory")
            {
                continue;
            }

            // Set table name to pluralized snake_case
            entityType.SetTableName(entityType.ClrType.Name
                .Pluralize(inputIsKnownToBeSingular: false)
                .Underscore().ToLowerInvariant());

            // Set column names to snake_case
            foreach (var property in entityType.GetProperties())
            {
                property.SetColumnName(property.Name.Underscore().ToLowerInvariant());
            }

            // Set primary keys to snake_case
            foreach (var key in entityType.GetKeys())
            {
                if (key.IsPrimaryKey())
                {
                    key.SetName(key.GetName().Underscore().ToLowerInvariant());
                }
            }

            // Set index names to snake_case
            foreach (var index in entityType.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName().Underscore().ToLowerInvariant());
            }

            // Set foreign key constraint names to snake_case
            foreach (var foreignKey in entityType.GetForeignKeys())
            {
                foreignKey.SetConstraintName(foreignKey.GetConstraintName().Underscore().ToLowerInvariant());
            }
        }
        
    }

    private static List<Book> GenerateData()
    {
        List<string> genres = ["Fantasy", "Sci-Fi", "Mystery", "Romance", "Horror", "Non-fiction"];

        return new Bogus.Faker<Book>()
            .RuleFor(b => b.Id, f => f.IndexFaker + 1)
            .RuleFor(b => b.Isbn, _ => Guid.NewGuid().ToString())
            .RuleFor(b => b.Title, f => f.Lorem.Sentence(3))
            .RuleFor(b => b.Genre, f => f.PickRandom(genres))
            .RuleFor(b => b.Author, f => f.Name.FullName())
            .RuleFor(b => b.Price, f => f.Random.Int(100, 1000))
            .Generate(20);
    }
    
    
}
using System.Reflection;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using SqlScribe.ExampleScaffolder.Domain;

namespace SqlScribe.ExampleScaffolder.Persistence;

public class BookDbContext(DbContextOptions<BookDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Author> Authors => Set<Author>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        var authorData = DataSeeder.GenerateAuthorData();
        modelBuilder.Entity<Author>().HasData(authorData);
        modelBuilder.Entity<Book>().HasData(DataSeeder.GenerateBookData(authorData));

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
}
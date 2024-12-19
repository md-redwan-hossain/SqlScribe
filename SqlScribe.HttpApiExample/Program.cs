using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Scalar.AspNetCore;
using SqlScribe.Enums;
using SqlScribe.ExampleScaffolder.Persistence;
using SqlScribe.Factories;
using SqlScribe.HttpApiExample.Controllers;
using SqlScribe.HttpApiExample.Services;
using SqlScribe.HttpApiExample.Utils;

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

var builder = WebApplication.CreateBuilder(args);

const string connectionString = "DataSource=db.sqlite3;Cache=Shared;";

var optionsBuilder = new DbContextOptionsBuilder<BookDbContext>();
optionsBuilder.UseSqlite(connectionString);

builder.Services.TryAddSingleton<IDbConnectionFactory>(_ => new SqliteConnectionFactory(connectionString));
await using (var dbContext = new BookDbContext(optionsBuilder.Options))
{
    dbContext.Database.EnsureDeleted();
    dbContext.Database.EnsureCreated();
    dbContext.Database.OpenConnection();
    dbContext.Database.ExecuteSqlRaw("PRAGMA journal_mode=DELETE;");
    dbContext.Database.CloseConnection();
}

builder.Services.AddSingleton<SqlQueryBuilderFactory>(_ =>
    new SqlQueryBuilderFactory(DatabaseVendor.SqLite, SqlNamingConvention.LowerSnakeCase, true)
);

builder.Services.TryAddScoped<IBookService, BookService>();
builder.Services.TryAddSingleton<IClientErrorFactory, ClientErrorFactory>();

builder.Services.AddDbContext<BookDbContext>(opts => opts.UseSqlite(connectionString));

builder.Services.AddControllers(opts =>
    {
        opts.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
        opts.OutputFormatters.RemoveType<StringOutputFormatter>();
        opts.ModelMetadataDetailsProviders.Add(new SystemTextJsonValidationMetadataProvider());
    })
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        opts.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(e => e.Value is { Errors.Count: > 0 })
                .Select(e => new
                {
                    Field = JsonNamingPolicy.CamelCase.ConvertName(e.Key),
                    Errors = e.Value?.Errors.Select(er => er.ErrorMessage)
                })
                .ToList();

            return context.MakeResponse(
                StatusCodes.Status400BadRequest,
                errors,
                "One or more validation errors occurred."
            );
        };
    });

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.Lifetime.ApplicationStopping.Register(() =>
{
    var dbPath = Path.Combine(builder.Environment.ContentRootPath, "book_db.sqlite3");
    if (File.Exists(dbPath)) File.Delete(dbPath);
});

app.MapControllers();
app.Run();
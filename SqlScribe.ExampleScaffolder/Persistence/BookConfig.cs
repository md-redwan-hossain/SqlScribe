using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SqlScribe.ExampleScaffolder.Domain;

namespace SqlScribe.ExampleScaffolder.Persistence;

public class BookConfig : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(256);
        builder.Property(x => x.Isbn).HasMaxLength(38);
        builder.Property(x => x.Genre).HasMaxLength(256);

        builder
            .HasOne<Author>()
            .WithMany()
            .HasForeignKey(x => x.AuthorId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);
    }
}
using Company.Template.Domain.Products;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Company.Template.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);

        builder.Property(product => product.Id)
               .HasConversion(id => id.Value, value => ProductId.From(value))
               .ValueGeneratedNever();

        builder.ComplexProperty(product => product.Name, name =>
        {
            name.Property(value => value.Value)
                .HasColumnName("name")
                .HasMaxLength(ProductName.MaxLength)
                .IsRequired();
        });

        builder.ComplexProperty(product => product.Price, price =>
        {
            price.Property(value => value.Amount)
                 .HasColumnName("price_amount")
                 .HasPrecision(18, 2)
                 .IsRequired();

            price.Property(value => value.Currency)
                 .HasColumnName("price_currency")
                 .HasMaxLength(Currency.CodeLength)
                 .HasConversion(
                    currency => currency.Code,
                    code => string.IsNullOrEmpty(code)
                        ? Currency.Empty
                        : Currency.Create(code))
                .IsRequired();
        });

        builder.Property(product => product.Status)
               .HasConversion<string>()
               .HasMaxLength(32)
               .IsRequired();

        builder.Property(product => product.CreatedAt)
               .IsRequired();

        builder.Property(product => product.DiscontinuedAt);

        builder.Ignore(product => product.DomainEvents);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OffersService.Models;

namespace OffersService.Data.Configurations;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> entity)
    {
        entity.ToTable(TableNames.Offers);

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Price)
            .HasColumnType("decimal(10,2)");

        entity.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.HasOne(e => e.Product)
            .WithMany(p => p.Offers)
            .HasForeignKey(e => e.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasIndex(e => e.ProductId);

        entity.HasData(
            new Offer { Id = 1, ProductId = 1, Price = 0.99m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 2, ProductId = 2, Price = 1.49m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 3, ProductId = 3, Price = 1.29m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}

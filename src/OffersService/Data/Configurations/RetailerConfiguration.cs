using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OffersService.Models;

namespace OffersService.Data.Configurations;

public class RetailerConfiguration : IEntityTypeConfiguration<Retailer>
{
    public void Configure(EntityTypeBuilder<Retailer> entity)
    {
        entity.ToTable(TableNames.Retailers);

        entity.HasKey(e => e.Id);

        entity.Property(e => e.Name)
            .HasMaxLength(200)
            .IsRequired();

        entity.Property(e => e.Country)
            .HasColumnType("char(2)")
            .HasMaxLength(2)
            .IsFixedLength()
            .IsRequired();

        entity.Property(e => e.IsActive)
            .HasDefaultValue(true);

        entity.Property(e => e.CreatedAt)
            .HasColumnType("datetime");

        entity.HasIndex(e => e.Name)
            .IsUnique();

        entity.HasData(
            new Retailer { Id = 1, Name = "Rewe", Country = "AT", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Retailer { Id = 2, Name = "Billa", Country = "AT", IsActive = true, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}

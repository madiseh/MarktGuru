using Microsoft.EntityFrameworkCore;
using OffersService.Models;

namespace OffersService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Offer> Offers => Set<Offer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Category).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Offer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Offers)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ProductId);
        });

        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Whole Milk 1L",       Category = "Dairy",     CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 2, Name = "Sourdough Bread 500g", Category = "Bakery",    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 3, Name = "Orange Juice 1L",      Category = "Beverages", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        // Seed Offers
        modelBuilder.Entity<Offer>().HasData(
            new Offer { Id = 1, ProductId = 1, Price = 0.99m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active,  CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 2, ProductId = 2, Price = 1.49m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active,  CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 3, ProductId = 3, Price = 1.29m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active,  CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
    }
}

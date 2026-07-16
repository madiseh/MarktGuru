using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.Models;
using OffersService.Services;
using Xunit;

namespace OffersService.Tests.Unit;

public class OfferServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("OfferServiceTest_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetActiveOffersAsync_ReturnsOnlyOffersWithFutureValidTo()
    {
        // Arrange
        await using var context = CreateContext();

        var product = new Product
        {
            Id        = 1,
            Name      = "Test Product",
            Category  = "Test",
            CreatedAt = DateTime.UtcNow
        };
        context.Products.Add(product);

        context.Offers.AddRange(
            new Offer
            {
                Id        = 1,
                ProductId = 1,
                Price     = 1.00m,
                ValidFrom = DateTime.UtcNow.AddDays(-10),
                ValidTo   = DateTime.UtcNow.AddDays(-1),   // expired
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Offer
            {
                Id        = 2,
                ProductId = 1,
                Price     = 2.00m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo   = DateTime.UtcNow.AddDays(10),   // valid
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new OfferService(context);

        // Act
        var result = await service.GetActiveOffersAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(2, result.First().Id);
    }

    [Fact]
    public async Task GetActiveOffersAsync_ExcludesOffersBeforeValidFrom()
    {
        // Arrange
        await using var context = CreateContext();

        context.Products.Add(new Product
        {
            Id        = 1,
            Name      = "Test Product",
            Category  = "Test",
            CreatedAt = DateTime.UtcNow
        });

        context.Offers.AddRange(
            new Offer
            {
                Id        = 1,
                ProductId = 1,
                Price     = 1.00m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo   = DateTime.UtcNow.AddDays(1),
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Offer
            {
                Id        = 2,
                ProductId = 1,
                Price     = 2.00m,
                ValidFrom = DateTime.UtcNow.AddDays(1),
                ValidTo   = DateTime.UtcNow.AddDays(10),
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new OfferService(context);

        // Act
        var result = await service.GetActiveOffersAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenOfferDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new OfferService(context);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetActiveOffersAsync_ReturnsAllSeedOffers()
    {
        // Arrange
        await using var context = CreateContext();

        context.Products.AddRange(
            new Product { Id = 1, Name = "Whole Milk 1L",        Category = "Dairy",     CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 2, Name = "Sourdough Bread 500g", Category = "Bakery",    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Product { Id = 3, Name = "Orange Juice 1L",      Category = "Beverages", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );

        context.Offers.AddRange(
            new Offer { Id = 1, ProductId = 1, Price = 0.99m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 2, ProductId = 2, Price = 1.49m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2024, 1,  2, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
            new Offer { Id = 3, ProductId = 3, Price = 1.29m, ValidFrom = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc), ValidTo = new DateTime(2099, 12, 31, 0, 0, 0, DateTimeKind.Utc), Status = OfferStatus.Active, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
        );
        await context.SaveChangesAsync();

        var service = new OfferService(context);

        // Act
        var result = await service.GetActiveOffersAsync();

        var expectedActiveOfferIds = await context.Offers
            .Where(offer => offer.ValidFrom <= DateTime.UtcNow && offer.ValidTo >= DateTime.UtcNow && offer.Status == OfferStatus.Active)
            .Select(offer => offer.Id)
            .ToListAsync();

        Assert.Equal(expectedActiveOfferIds.Count, result.Count());
        Assert.Equal(expectedActiveOfferIds.OrderBy(id => id), result.Select(offer => offer.Id).OrderBy(id => id));
    }
}

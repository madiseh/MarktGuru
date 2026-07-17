using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.Models;
using OffersService.Services;
using Xunit;

namespace OffersService.Tests.Unit;

public class RetailerServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("RetailerServiceTest_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsAggregatedStats_ForActiveRetailer()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTime.UtcNow;

        context.Products.AddRange(
            new Product { Id = 1, Name = "Milk", Category = "Dairy", CreatedAt = now },
            new Product { Id = 2, Name = "Bread", Category = "Bakery", CreatedAt = now },
            new Product { Id = 3, Name = "Juice", Category = "Beverages", CreatedAt = now });

        context.Retailers.Add(new Retailer
        {
            Id = 1,
            Name = "Rewe",
            Country = "AT",
            IsActive = true,
            CreatedAt = now
        });

        context.Offers.AddRange(
            new Offer
            {
                Id = 1,
                ProductId = 1,
                RetailerId = 1,
                Price = 1.00m,
                ValidFrom = now.AddDays(-2),
                ValidTo = now.AddDays(2),
                Status = OfferStatus.Active,
                CreatedAt = now
            },
            new Offer
            {
                Id = 2,
                ProductId = 2,
                RetailerId = 1,
                Price = 3.00m,
                ValidFrom = now.AddDays(-1),
                ValidTo = now.AddDays(3),
                Status = OfferStatus.Active,
                CreatedAt = now
            },
            new Offer
            {
                Id = 3,
                ProductId = 3,
                RetailerId = 1,
                Price = 0.50m,
                ValidFrom = now.AddDays(-5),
                ValidTo = now.AddDays(-1),
                Status = OfferStatus.Active,
                CreatedAt = now
            },
            new Offer
            {
                Id = 4,
                ProductId = 3,
                RetailerId = 1,
                Price = 10.00m,
                ValidFrom = now.AddDays(-1),
                ValidTo = now.AddDays(1),
                Status = OfferStatus.Pending,
                CreatedAt = now
            });

        await context.SaveChangesAsync();
        var service = new RetailerService(context);

        // Act
        var result = await service.GetStatsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Rewe", result.Name);
        Assert.Equal("AT", result.Country);
        Assert.True(result.IsActive);
        Assert.Equal(2, result.ActiveOfferCount);
        Assert.Equal(2.00m, result.AvgPrice);
        Assert.Equal(1.00m, result.LowestPrice);

        Assert.NotNull(result.MostRecentActiveOffer);
        Assert.Equal(2, result.MostRecentActiveOffer.Id);
        Assert.Equal("Bread", result.MostRecentActiveOffer.ProductName);
        Assert.Equal(3.00m, result.MostRecentActiveOffer.Price);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsNull_WhenRetailerDoesNotExist()
    {
        // Arrange
        await using var context = CreateContext();
        var service = new RetailerService(context);

        // Act
        var result = await service.GetStatsAsync(9999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsNull_WhenRetailerIsInactive()
    {
        // Arrange
        await using var context = CreateContext();

        context.Retailers.Add(new Retailer
        {
            Id = 1,
            Name = "Inactive Retailer",
            Country = "AT",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new RetailerService(context);

        // Act
        var result = await service.GetStatsAsync(1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatsAsync_ReturnsEmptyOfferStats_WhenRetailerHasNoActiveOffers()
    {
        // Arrange
        await using var context = CreateContext();
        var now = DateTime.UtcNow;

        context.Products.Add(new Product
        {
            Id = 1,
            Name = "Milk",
            Category = "Dairy",
            CreatedAt = now
        });

        context.Retailers.Add(new Retailer
        {
            Id = 1,
            Name = "Billa",
            Country = "AT",
            IsActive = true,
            CreatedAt = now
        });

        context.Offers.Add(new Offer
        {
            Id = 1,
            ProductId = 1,
            RetailerId = 1,
            Price = 1.00m,
            ValidFrom = now.AddDays(-3),
            ValidTo = now.AddDays(-1),
            Status = OfferStatus.Active,
            CreatedAt = now
        });

        await context.SaveChangesAsync();
        var service = new RetailerService(context);

        // Act
        var result = await service.GetStatsAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(0, result.ActiveOfferCount);
        Assert.Null(result.AvgPrice);
        Assert.Null(result.LowestPrice);
        Assert.Null(result.MostRecentActiveOffer);
    }
}

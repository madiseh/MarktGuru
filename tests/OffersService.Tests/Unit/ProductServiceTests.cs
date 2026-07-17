using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.Models;
using OffersService.Services;
using Xunit;

namespace OffersService.Tests.Unit;

public class ProductServiceTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("ProductServiceTest_" + Guid.NewGuid())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetProductsWithOffersAsync_ReturnsProductsWithTheirOffers()
    {
        // Arrange
        await using var context = CreateContext();

        context.Products.AddRange(
            new Product
            {
                Id        = 1,
                Name      = "Whole Milk 1L",
                Category  = "Dairy",
                CreatedAt = DateTime.UtcNow
            },
            new Product
            {
                Id        = 2,
                Name      = "Sourdough Bread 500g",
                Category  = "Bakery",
                CreatedAt = DateTime.UtcNow
            }
        );

        context.Offers.AddRange(
            new Offer
            {
                Id        = 1,
                ProductId = 1,
                Price     = 0.99m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo   = DateTime.UtcNow.AddDays(1),
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            },
            new Offer
            {
                Id        = 2,
                ProductId = 2,
                Price     = 1.49m,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo   = DateTime.UtcNow.AddDays(1),
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            }
        );
        await context.SaveChangesAsync();

        var service = new ProductService(context);

        // Act
        var result = (await service.GetProductsWithOffersAsync()).ToArray();

        // Assert
        Assert.Equal(2, result.Length);
        Assert.All(result, product => Assert.Single(product.Offers));
        Assert.Contains(result, product => product.Id == 1 && product.Offers.Single().Id == 1);
        Assert.Contains(result, product => product.Id == 2 && product.Offers.Single().Id == 2);
    }
}

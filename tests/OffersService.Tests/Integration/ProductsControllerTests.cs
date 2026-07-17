using System.Net;
using System.Text.Json;
using OffersService.Tests.Helpers;
using Xunit;

namespace OffersService.Tests.Integration;

public class ProductsControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public ProductsControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProducts_ReturnsProductsWithOffers_InResponseBody()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(body), "Response body should contain products but was empty.");

        using var json = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);
        Assert.Equal(3, json.RootElement.GetArrayLength());

        var milk = json.RootElement
            .EnumerateArray()
            .Single(product => product.GetProperty("id").GetInt32() == 1);

        Assert.Equal("Whole Milk 1L", milk.GetProperty("name").GetString());
        Assert.Equal("Dairy", milk.GetProperty("category").GetString());

        var offers = milk.GetProperty("offers");
        Assert.Equal(JsonValueKind.Array, offers.ValueKind);
        Assert.Contains(
            offers.EnumerateArray(),
            offer => offer.GetProperty("id").GetInt32() == 1 &&
                     offer.GetProperty("price").GetDecimal() == 0.99m);
    }
}

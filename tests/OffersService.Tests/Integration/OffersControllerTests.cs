using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;
using OffersService.Tests.Helpers;
using Xunit;

namespace OffersService.Tests.Integration;

public class OffersControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public OffersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetActiveOffers_ReturnsActiveOffers_InResponseBody()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/offers/active");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(body), "Response body should contain active offers but was empty.");

        using var json = JsonDocument.Parse(body);
        Assert.Equal(JsonValueKind.Array, json.RootElement.ValueKind);

        var offerIds = json.RootElement
            .EnumerateArray()
            .Select(offer => offer.GetProperty("id").GetInt32())
            .ToArray();

        Assert.Contains(1, offerIds);
        Assert.Contains(3, offerIds);
        Assert.DoesNotContain(2, offerIds);
    }

    [Fact]
    public async Task ImportOffers_ReturnsImportedCount_InResponseBody()
    {
        // Arrange
        var client = _factory.CreateClient();

        var request = new ImportRequest
        {
            Rows = new[]
            {
                new OfferRow
                {
                    ProductId = 1,
                    Price     = 2.49m,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo   = DateTime.UtcNow.AddDays(7)
                }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/offers/import", request);
        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        // Assert — the response body should include the count of imported rows so the
        // caller knows whether the import succeeded. Because the controller fires async void
        // and immediately returns Accepted() with no body, this will be null.
        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(body), "Response body should contain import result but was empty.");

        using var json = JsonDocument.Parse(body);
        Assert.Equal(request.Rows.Count(), json.RootElement.GetProperty("importedCount").GetInt32());
    }

    [Fact]
    public async Task ImportOffers_Returns400_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        var request = new ImportRequest
        {
            Rows = new[]
            {
                new OfferRow
                {
                    ProductId = 9999, // does not exist — FK violation
                    Price     = 1.00m,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo   = DateTime.UtcNow.AddDays(1)
                }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/offers/import", request);

        // Assert — expects 500 from FK violation, but gets 202 because async void swallows the exception
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

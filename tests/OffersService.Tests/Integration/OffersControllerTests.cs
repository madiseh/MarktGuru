using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using OffersService.DTOs;
using OffersService.Tests.Helpers;
using OffersService.Validators;
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

        // Assert
        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrEmpty(body), "Response body should contain import result but was empty.");

        using var json = JsonDocument.Parse(body);
        Assert.Equal(request.Rows!.Count(), json.RootElement.GetProperty("importedCount").GetInt32());
    }

    [Fact]
    public async Task ImportOffers_Returns400_WhenProductDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();
        const int missingProductId = 9999;

        var request = new ImportRequest
        {
            Rows = new[]
            {
                new OfferRow
                {
                    ProductId = missingProductId,
                    Price     = 1.00m,
                    ValidFrom = DateTime.UtcNow,
                    ValidTo   = DateTime.UtcNow.AddDays(1)
                }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/offers/import", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var errors = json.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("productid", out var productIdErrors));
        Assert.Contains(
            productIdErrors.EnumerateArray(),
            error => error.GetString() == ImportValidationMessages.ProductIdsDoNotExist(new[] { missingProductId }));
    }

    [Fact]
    public async Task ImportOffers_Returns400_WhenRowsIsNull()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { rows = (OfferRow[]?)null };

        // Act
        var response = await client.PostAsJsonAsync("/api/offers/import", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var errors = json.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("rows", out var rowErrors));
        Assert.Contains(rowErrors.EnumerateArray(), error => error.GetString() == ImportValidationMessages.RowsRequired);
    }
}

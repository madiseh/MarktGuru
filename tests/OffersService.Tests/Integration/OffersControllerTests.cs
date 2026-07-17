using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OffersService.DTOs;
using OffersService.Tests.Helpers;
using OffersService.Validators;
using Xunit;
using static OffersService.Tests.Helpers.OffersControllerTestHelper;

namespace OffersService.Tests.Integration;

public class OffersControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public OffersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Performance

    [Theory]
    [InlineData("/api/offers?page=1&pageSize=1", 1)]
    [InlineData("/api/offers?productId=9999", 1)]
    public async Task OffersApi_ExecutesExpectedDatabaseCommandCount(string url, int expectedCommandCount)
    {
        // Arrange
        var client = _factory.CreateClient();
        var commandCounter = _factory.Services.GetRequiredService<DbCommandCounterInterceptor>();
        commandCounter.Reset();

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedCommandCount, commandCounter.ExecutedCount);
    }

    #endregion Performance

    #region GET /api/offers

    [Theory]
    [InlineData("", "1,3")]
    [InlineData("?page=2&pageSize=1", "3")]
    [InlineData("?status=all", "1,2,3")]
    [InlineData("?productId=1", "1")]
    [InlineData("?retailerId=1", "1,3")]
    [InlineData("?status=expired&productId=2&retailerId=2", "2")]
    public async Task GetOffers_ReturnsExpectedOfferIds(
        string queryString,
        string expectedOfferIds)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/offers{queryString}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(ParseIds(expectedOfferIds), GetOfferIds(json.RootElement));
    }

    [Theory]
    [InlineData("", 2, 1, 20, false, false)]
    [InlineData("?page=1&pageSize=1", 2, 1, 1, true, false)]
    [InlineData("?page=2&pageSize=1", 2, 2, 1, false, true)]
    public async Task GetOffers_ReturnsExpectedPaginationMetadata(
        string queryString,
        int expectedTotal,
        int expectedPage,
        int expectedPageSize,
        bool expectedHasNextPage,
        bool expectedHasPreviousPage)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/offers{queryString}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal(expectedTotal, json.RootElement.GetProperty(PagedListProperty.Total).GetInt32());
        Assert.Equal(expectedPage, json.RootElement.GetProperty(PagedListProperty.Page).GetInt32());
        Assert.Equal(expectedPageSize, json.RootElement.GetProperty(PagedListProperty.PageSize).GetInt32());
        Assert.Equal(expectedHasNextPage, json.RootElement.GetProperty(PagedListProperty.HasNextPage).GetBoolean());
        Assert.Equal(expectedHasPreviousPage, json.RootElement.GetProperty(PagedListProperty.HasPreviousPage).GetBoolean());
    }

    [Fact]
    public async Task GetOffers_ReturnsEmptyPage_WhenNoOffersMatch()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/offers?productId=9999");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Empty(GetOfferIds(json.RootElement));
        Assert.Equal(0, json.RootElement.GetProperty(PagedListProperty.Total).GetInt32());
        Assert.Equal(1, json.RootElement.GetProperty(PagedListProperty.Page).GetInt32());
        Assert.Equal(20, json.RootElement.GetProperty(PagedListProperty.PageSize).GetInt32());
        Assert.False(json.RootElement.GetProperty(PagedListProperty.HasNextPage).GetBoolean());
        Assert.False(json.RootElement.GetProperty(PagedListProperty.HasPreviousPage).GetBoolean());
    }

    [Theory]
    [InlineData("?pageSize=101", "pagesize", OfferListValidationMessages.PageSizeMustNotExceedLimit)]
    [InlineData("?pageSize=0", "pagesize", OfferListValidationMessages.PageSizeMustBePositive)]
    [InlineData("?status=pending", "status", null)]
    [InlineData("?status=99", "status", null)]
    [InlineData("?page=2147483647&pageSize=2", "page", OfferListValidationMessages.OffsetMustNotBeTooLarge)]
    [InlineData("?page=0", "page", OfferListValidationMessages.PageMustBePositive)]
    [InlineData("?productId=0", "productid", OfferListValidationMessages.ProductIdMustBePositive)]
    [InlineData("?retailerId=-1", "retailerid", OfferListValidationMessages.RetailerIdMustBePositive)]
    public async Task GetOffers_Returns400_WhenQueryIsInvalid(
        string queryString,
        string errorProperty,
        string? errorMessage)
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync($"/api/offers{queryString}");

        // Assert
        await AssertBadRequestErrorAsync(
            response,
            errorProperty,
            errorMessage);
    }

    #endregion GET /api/offers

    

    #region GET /api/offers/active

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

    #endregion GET /api/offers/active

    #region POST /api/offers/import

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

    [Fact]
    public async Task ImportOffers_Returns400_WhenRowsContainsNull()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new { rows = new object?[] { null } };

        // Act
        var response = await client.PostAsJsonAsync("/api/offers/import", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        var errors = json.RootElement.GetProperty("errors");
        Assert.True(errors.TryGetProperty("rows[0]", out var rowErrors));
        Assert.Contains(rowErrors.EnumerateArray(), error => error.GetString() == ImportValidationMessages.OfferRowRequired);
    }

    #endregion POST /api/offers/import
}

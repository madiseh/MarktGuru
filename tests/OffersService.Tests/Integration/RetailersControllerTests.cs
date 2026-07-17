using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;
using OffersService.Tests.Helpers;
using OffersService.Validators;
using Xunit;

namespace OffersService.Tests.Integration;

public class RetailersControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public RetailersControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    #region Performance

    [Fact]
    public async Task RetailersApi_ExecutesExpectedDatabaseCommandCount()
    {
        // Arrange
        var client = _factory.CreateClient();
        var commandCounter = _factory.Services.GetRequiredService<DbCommandCounterInterceptor>();
        commandCounter.Reset();

        // Act
        var response = await client.GetAsync("/api/retailers/1/stats");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, commandCounter.ExecutedCount);
    }

    #endregion Performance

    #region POST /api/retailers

    [Fact]
    public async Task CreateRetailer_Returns201WithLocationHeaderAndResponseBody()
    {
        // Arrange
        var client = _factory.CreateClient();
        var retailerName = $"Test Retailer {Guid.NewGuid():N}";
        var request = new CreateRetailerRequest
        {
            Name = $" {retailerName} ",
            Country = "at"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/retailers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        var retailerId = root.GetProperty("id").GetInt32();
        Assert.True(retailerId > 0);
        Assert.Equal(retailerName, root.GetProperty("name").GetString());
        Assert.Equal("AT", root.GetProperty("country").GetString());
        Assert.True(root.GetProperty("isActive").GetBoolean());
        Assert.EndsWith("Z", root.GetProperty("createdAt").GetString());
        Assert.EndsWith($"/api/retailers/{retailerId}", response.Headers.Location!.OriginalString);

        var getResponse = await client.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var getBody = await getResponse.Content.ReadAsStringAsync();
        using var getJson = JsonDocument.Parse(getBody);

        Assert.Equal(retailerId, getJson.RootElement.GetProperty("id").GetInt32());
        Assert.Equal(retailerName, getJson.RootElement.GetProperty("name").GetString());
        Assert.Equal("AT", getJson.RootElement.GetProperty("country").GetString());
    }

    [Fact]
    public async Task CreateRetailer_Returns409_WhenNameAlreadyExists()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateRetailerRequest
        {
            Name = "Rewe",
            Country = "AT"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/retailers", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("Retailer already exists", json.RootElement.GetProperty("title").GetString());
        Assert.Equal(RetailerValidationMessages.NameAlreadyExists, json.RootElement.GetProperty("detail").GetString());
        Assert.Equal((int)HttpStatusCode.Conflict, json.RootElement.GetProperty("status").GetInt32());
    }

    [Fact]
    public async Task CreateRetailer_Returns400_WhenCountryIsInvalid()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreateRetailerRequest
        {
            Name = $"Invalid Country Retailer {Guid.NewGuid():N}",
            Country = "AUT"
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/retailers", request);

        // Assert
        await AssertValidationErrorAsync(
            response,
            "country",
            RetailerValidationMessages.CountryMustBeIsoCode);
    }

    #endregion POST /api/retailers

    #region GET /api/retailers/{id}/stats

    [Fact]
    public async Task GetRetailerStats_ReturnsAggregatedStats_ForActiveRetailer()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/retailers/1/stats");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal(1, root.GetProperty("id").GetInt32());
        Assert.Equal("Rewe", root.GetProperty("name").GetString());
        Assert.Equal("AT", root.GetProperty("country").GetString());
        Assert.True(root.GetProperty("isActive").GetBoolean());
        Assert.Equal(2, root.GetProperty("activeOfferCount").GetInt32());
        Assert.Equal(1.14m, root.GetProperty("avgPrice").GetDecimal());
        Assert.Equal(0.99m, root.GetProperty("lowestPrice").GetDecimal());

        var mostRecentActiveOffer = root.GetProperty("mostRecentActiveOffer");
        Assert.Equal(3, mostRecentActiveOffer.GetProperty("id").GetInt32());
        Assert.Equal("Orange Juice 1L", mostRecentActiveOffer.GetProperty("productName").GetString());
        Assert.Equal(1.29m, mostRecentActiveOffer.GetProperty("price").GetDecimal());
        Assert.EndsWith("Z", mostRecentActiveOffer.GetProperty("validTo").GetString());
    }

    [Fact]
    public async Task GetRetailerStats_ReturnsNullOfferStats_WhenRetailerHasNoActiveOffers()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/retailers/2/stats");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var root = json.RootElement;

        Assert.Equal(2, root.GetProperty("id").GetInt32());
        Assert.Equal("Billa", root.GetProperty("name").GetString());
        Assert.Equal(0, root.GetProperty("activeOfferCount").GetInt32());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("avgPrice").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("lowestPrice").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("mostRecentActiveOffer").ValueKind);
    }

    [Fact]
    public async Task GetRetailerStats_Returns404_WhenRetailerDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/retailers/9999/stats");

        // Assert
        await AssertNotFoundProblemAsync(response);
    }

    [Fact]
    public async Task GetRetailerStats_Returns404_WhenRetailerIsInactive()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AddInactiveRetailerAsync(9001);

        // Act
        var response = await client.GetAsync("/api/retailers/9001/stats");

        // Assert
        await AssertNotFoundProblemAsync(response);
    }

    [Fact]
    public async Task GetRetailerStats_Returns400_WhenRetailerIdIsNotAnInteger()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/retailers/abc/stats");

        // Assert
        await AssertValidationErrorAsync(
            response,
            "id",
            RetailerValidationMessages.IdMustBePositiveInteger);
    }

    [Fact]
    public async Task GetRetailerStats_Returns400_WhenRetailerIdIsLessThanOne()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/retailers/0/stats");

        // Assert
        await AssertValidationErrorAsync(
            response,
            "id",
            RetailerValidationMessages.IdMustBePositiveInteger);
    }

    #endregion GET /api/retailers/{id}/stats

    private async Task AddInactiveRetailerAsync(int id)
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (await context.Retailers.FindAsync(id) is not null)
        {
            return;
        }

        context.Retailers.Add(new Retailer
        {
            Id = id,
            Name = $"Inactive Retailer {id}",
            Country = "AT",
            IsActive = false,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
    }

    private static async Task AssertNotFoundProblemAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);

        Assert.Equal("Retailer not found", json.RootElement.GetProperty("title").GetString());
        Assert.Equal(RetailerValidationMessages.NotFoundOrInactive, json.RootElement.GetProperty("detail").GetString());
        Assert.Equal((int)HttpStatusCode.NotFound, json.RootElement.GetProperty("status").GetInt32());
    }

    private static async Task AssertValidationErrorAsync(
        HttpResponseMessage response,
        string propertyName,
        string expectedMessage)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var errors = json.RootElement.GetProperty("errors");

        Assert.True(errors.TryGetProperty(propertyName, out var propertyErrors));
        Assert.Contains(
            propertyErrors.EnumerateArray(),
            error => error.GetString() == expectedMessage);
    }
}

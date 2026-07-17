using System.Net;
using System.Text.Json;
using OffersService.DTOs;
using OffersService.Models.Pagination;
using Xunit;

namespace OffersService.Tests.Helpers;

public static class OffersControllerTestHelper
{
    public static int[] GetOfferIds(JsonElement root)
    {
        return root
            .GetProperty(PagedListProperty.Items)
            .EnumerateArray()
            .Select(offer => offer.GetProperty("id").GetInt32())
            .ToArray();
    }

    public static int[] ParseIds(string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();
    }

    public static async Task AssertBadRequestErrorAsync(
        HttpResponseMessage response,
        string errorProperty,
        string? errorMessage = null)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(body);
        var errors = json.RootElement.GetProperty("errors");
        var propertyErrors = GetErrorsForProperty(errors, errorProperty);

        Assert.NotEmpty(propertyErrors);
        if (errorMessage is not null)
        {
            Assert.Contains(errorMessage, propertyErrors);
        }
    }

    private static string[] GetErrorsForProperty(JsonElement errors, string propertyName)
    {
        foreach (var errorProperty in errors.EnumerateObject())
        {
            if (string.Equals(errorProperty.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return errorProperty.Value
                    .EnumerateArray()
                    .Select(error => error.GetString() ?? string.Empty)
                    .ToArray();
            }
        }

        return Array.Empty<string>();
    }

    public static class PagedListProperty
    {
        public static readonly string Items = ToJsonPropertyName(nameof(PagedList<OfferListItemDto>.Items));
        public static readonly string Page = ToJsonPropertyName(nameof(PagedList<OfferListItemDto>.Page));
        public static readonly string PageSize = ToJsonPropertyName(nameof(PagedList<OfferListItemDto>.PageSize));
        public static readonly string TotalCount = ToJsonPropertyName(nameof(PagedList<OfferListItemDto>.TotalCount));
        public static readonly string HasNextPage = ToJsonPropertyName(nameof(PagedList<OfferListItemDto>.HasNextPage));
        public static readonly string HasPreviousPage = ToJsonPropertyName(nameof(PagedList<OfferListItemDto>.HasPreviousPage));

        private static string ToJsonPropertyName(string propertyName)
        {
            return JsonNamingPolicy.CamelCase.ConvertName(propertyName);
        }
    }
}

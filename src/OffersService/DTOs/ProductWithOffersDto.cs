using OffersService.Models;

namespace OffersService.DTOs;

public record OfferSummaryDto(
    int Id,
    decimal Price,
    DateTime ValidFrom,
    DateTime ValidTo);

public record ProductWithOffersDto(
    int Id,
    string Name,
    string Category,
    IEnumerable<OfferSummaryDto> Offers)
{
    public ProductWithOffersDto(Product product) : this(
        product.Id,
        product.Name,
        product.Category,
        product.Offers.Select(o => new OfferSummaryDto(o.Id, o.Price, o.ValidFrom, o.ValidTo)))
    {
    }
}

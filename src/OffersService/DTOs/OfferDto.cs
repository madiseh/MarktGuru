using OffersService.Models;

namespace OffersService.DTOs;

public record OfferDto(
    int Id,
    int ProductId,
    string ProductName,
    decimal Price,
    DateTime ValidFrom,
    DateTime ValidTo,
    string Status)
{
    public OfferDto(Offer offer) : this(
        offer.Id,
        offer.ProductId,
        offer.Product?.Name ?? string.Empty,
        offer.Price,
        offer.ValidFrom,
        offer.ValidTo,
        offer.Status.ToString())
    {
    }
}

namespace OffersService.DTOs;

public record OfferListItemDto(
    int Id,
    int ProductId,
    string ProductName,
    int RetailerId,
    string RetailerName,
    decimal Price,
    DateTime ValidFrom,
    DateTime ValidTo,
    string Status);

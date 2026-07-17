namespace OffersService.DTOs;

public record MostRecentActiveOfferDto(
    int Id,
    string ProductName,
    decimal Price,
    DateTime ValidTo);

namespace OffersService.DTOs;

public record RetailerStatsDto(
    int Id,
    string Name,
    string Country,
    bool IsActive,
    int ActiveOfferCount,
    decimal? AvgPrice,
    decimal? LowestPrice,
    MostRecentActiveOfferDto? MostRecentActiveOffer);

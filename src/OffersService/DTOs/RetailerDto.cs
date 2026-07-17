namespace OffersService.DTOs;

public record RetailerDto(
    int Id,
    string Name,
    string Country,
    bool IsActive,
    DateTime CreatedAt);

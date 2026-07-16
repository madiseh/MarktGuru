using OffersService.DTOs;

namespace OffersService.Services;

public interface IOfferService
{
    Task<IEnumerable<OfferDto>> GetActiveOffersAsync();
    Task<OfferDto?> GetByIdAsync(int id);
}

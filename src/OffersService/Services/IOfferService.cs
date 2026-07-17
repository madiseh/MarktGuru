using OffersService.DTOs;
using OffersService.Models.Pagination;

namespace OffersService.Services;

public interface IOfferService
{
    Task<IEnumerable<OfferDto>> GetActiveOffersAsync();
    Task<OfferDto?> GetByIdAsync(int id);
    Task<PagedList<OfferListItemDto>> GetOffersAsync(OfferListRequest request);
}

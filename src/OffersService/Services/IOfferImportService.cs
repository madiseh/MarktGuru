using OffersService.DTOs;

namespace OffersService.Services;

public interface IOfferImportService
{
    Task<int> ImportAsync(IEnumerable<OfferRow> rows);
}

using OffersService.DTOs;

namespace OffersService.Services;

public interface IOfferImportService
{
    Task ImportAsync(IEnumerable<OfferRow> rows);
}

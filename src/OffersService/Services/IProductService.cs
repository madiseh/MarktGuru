using OffersService.DTOs;

namespace OffersService.Services;

public interface IProductService
{
    Task<IEnumerable<ProductWithOffersDto>> GetProductsWithOffersAsync();
}

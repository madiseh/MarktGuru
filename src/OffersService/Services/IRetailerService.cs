using OffersService.DTOs;

namespace OffersService.Services;

public interface IRetailerService
{
    Task<RetailerStatsDto?> GetStatsAsync(int id);
}

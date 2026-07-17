using OffersService.DTOs;

namespace OffersService.Services;

public interface IRetailerService
{
    Task<RetailerDto?> GetByIdAsync(int id);
    Task<RetailerDto?> CreateAsync(CreateRetailerRequest request);
    Task<RetailerStatsDto?> GetStatsAsync(int id);
}

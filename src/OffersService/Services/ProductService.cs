using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;

namespace OffersService.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductWithOffersDto>> GetProductsWithOffersAsync()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Include(product => product.Offers)
            .ToListAsync();

        return products.Select(p => new ProductWithOffersDto(p));
    }
}

using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

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
        var products = await _context.Products.ToListAsync();
        foreach (var product in products)
        {
            product.Offers = (ICollection<Offer>)await _context.Offers
                .Where(o => o.ProductId == product.Id)
                .ToListAsync();
        }
        return products.Select(p => new ProductWithOffersDto(p));
    }
}

using FluentValidation;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Models;

namespace OffersService.Services;

public class OfferImportService : IOfferImportService
{
    private readonly AppDbContext _context;
    private readonly IValidator<ImportRequest> _importRequestValidator;

    public OfferImportService(AppDbContext context, IValidator<ImportRequest> importRequestValidator)
    {
        _context = context;
        _importRequestValidator = importRequestValidator;
    }

    public async Task<int> ImportAsync(IEnumerable<OfferRow>? rows)
    {
        var offerRows = rows?.ToList();

        var validationResult = await _importRequestValidator.ValidateAsync(new ImportRequest { Rows = offerRows });
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        foreach (var row in offerRows!)
        {
            await _context.Offers.AddAsync(new Offer
            {
                ProductId = row.ProductId,
                Price     = row.Price,
                ValidFrom = row.ValidFrom,
                ValidTo   = row.ValidTo,
                Status    = OfferStatus.Active,
                CreatedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        return offerRows.Count;
    }
}

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OffersService.Data;
using OffersService.DTOs;
using OffersService.Extensions;

namespace OffersService.Validators;

public class ImportRequestValidator : AbstractValidator<ImportRequest>
{
    public ImportRequestValidator(AppDbContext context, IValidator<OfferRow> offerRowValidator)
    {
        RuleFor(request => request.Rows)
            .NotNull()
            .WithMessage(ImportValidationMessages.RowsRequired);

        RuleForEach(request => request.Rows)
            .NotNull()
            .WithMessage(ImportValidationMessages.OfferRowRequired)
            .When(request => request.Rows is not null);

        RuleForEach(request => request.Rows)
            .SetValidator(offerRowValidator)
            .When(request => request.Rows is not null);

        RuleFor(request => request.Rows)
            .CustomAsync(async (rows, validationContext, cancellationToken) =>
            {
                if (rows is null)
                {
                    return;
                }

                var productIds = rows
                    .Where(row => row is not null)
                    .Select(row => row.ProductId)
                    .Where(productId => productId > 0)
                    .Distinct()
                    .ToArray();

                if (productIds.Length == 0)
                {
                    return;
                }

                var existingProductIds = await context.Products
                    .WhereIdIn(productIds)
                    .Select(product => product.Id)
                    .ToListAsync(cancellationToken);

                var missingProductIds = productIds
                    .Except(existingProductIds)
                    .OrderBy(productId => productId)
                    .ToArray();

                if (missingProductIds.Length > 0)
                {
                    validationContext.AddFailure(
                        nameof(OfferRow.ProductId),
                        ImportValidationMessages.ProductIdsDoNotExist(missingProductIds));
                }
            });
    }
}

using FluentValidation;
using OffersService.DTOs;

namespace OffersService.Validators;

public class OfferRowValidator : AbstractValidator<OfferRow>
{
    public OfferRowValidator()
    {
        RuleFor(row => row.ProductId)
            .GreaterThan(0);

        RuleFor(row => row.Price)
            .GreaterThan(0);

        RuleFor(row => row.ValidTo)
            .GreaterThan(row => row.ValidFrom);
    }
}

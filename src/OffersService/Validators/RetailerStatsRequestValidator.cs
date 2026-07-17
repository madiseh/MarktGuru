using FluentValidation;
using OffersService.DTOs;

namespace OffersService.Validators;

public class RetailerStatsRequestValidator : AbstractValidator<RetailerStatsRequest>
{
    public RetailerStatsRequestValidator()
    {
        RuleFor(request => request.Id)
            .Must(BePositiveInteger)
            .WithMessage(RetailerValidationMessages.IdMustBePositiveInteger);
    }

    private static bool BePositiveInteger(string id)
    {
        return int.TryParse(id, out var retailerId) && retailerId > 0;
    }
}

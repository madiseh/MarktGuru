using FluentValidation;
using OffersService.DTOs;

namespace OffersService.Validators;

public class CreateRetailerRequestValidator : AbstractValidator<CreateRetailerRequest>
{
    public CreateRetailerRequestValidator()
    {
        RuleFor(request => request.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(RetailerValidationMessages.NameRequired)
            .MaximumLength(200)
            .WithMessage(RetailerValidationMessages.NameMustNotExceedMaxLength);

        RuleFor(request => request.Country)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(RetailerValidationMessages.CountryRequired)
            .Length(2)
            .WithMessage(RetailerValidationMessages.CountryMustBeIsoCode)
            .Matches("^[A-Za-z]{2}$")
            .WithMessage(RetailerValidationMessages.CountryMustBeIsoCode);
    }
}

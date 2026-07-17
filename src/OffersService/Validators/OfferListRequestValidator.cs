using FluentValidation;
using OffersService.DTOs;

namespace OffersService.Validators;

public class OfferListRequestValidator : AbstractValidator<OfferListRequest>
{
    public OfferListRequestValidator()
    {
        RuleFor(request => request.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage(OfferListValidationMessages.PageMustBePositive);

        RuleFor(request => request.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage(OfferListValidationMessages.PageSizeMustBePositive);

        RuleFor(request => request.PageSize)
            .LessThanOrEqualTo(100)
            .WithMessage(OfferListValidationMessages.PageSizeMustNotExceedLimit);

        RuleFor(request => request)
            .Must(request => (long)(request.Page - 1) * request.PageSize <= int.MaxValue)
            .When(request => request.Page > 0 && request.PageSize > 0)
            .WithName(nameof(OfferListRequest.Page))
            .WithMessage(OfferListValidationMessages.OffsetMustNotBeTooLarge);

        RuleFor(request => request.ProductId)
            .Must(productId => productId is null or > 0)
            .WithMessage(OfferListValidationMessages.ProductIdMustBePositive);

        RuleFor(request => request.RetailerId)
            .Must(retailerId => retailerId is null or > 0)
            .WithMessage(OfferListValidationMessages.RetailerIdMustBePositive);

        RuleFor(request => request.Status)
            .IsInEnum()
            .WithMessage(OfferListValidationMessages.StatusMustBeSupported);
    }
}

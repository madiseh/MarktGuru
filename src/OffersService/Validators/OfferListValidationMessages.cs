namespace OffersService.Validators;

public static class OfferListValidationMessages
{
    public const string PageMustBePositive = "page must be greater than or equal to 1.";
    public const string PageSizeMustBePositive = "pageSize must be greater than or equal to 1.";
    public const string PageSizeMustNotExceedLimit = "pageSize must not exceed 100.";
    public const string OffsetMustNotBeTooLarge = "page and pageSize produce an offset that is too large.";
    public const string ProductIdMustBePositive = "productId must be greater than 0.";
    public const string RetailerIdMustBePositive = "retailerId must be greater than 0.";
    public const string StatusMustBeSupported = "status must be active, expired, or all.";
}

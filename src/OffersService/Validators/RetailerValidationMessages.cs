namespace OffersService.Validators;

public static class RetailerValidationMessages
{
    public const string IdMustBePositiveInteger = "retailer id must be a positive integer.";
    public const string NotFoundOrInactive = "Retailer was not found or is inactive.";
    public const string NameRequired = "name is required.";
    public const string NameMustNotExceedMaxLength = "name must be 200 characters or fewer.";
    public const string CountryRequired = "country is required.";
    public const string CountryMustBeIsoCode = "country must be a 2-letter ISO 3166-1 alpha-2 code.";
    public const string NameAlreadyExists = "A retailer with the same name already exists.";
}

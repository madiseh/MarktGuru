namespace OffersService.Validators;

public static class ImportValidationMessages
{
    public const string RowsRequired = "Rows is required.";
    public const string OfferRowRequired = "Offer row is required.";
    public const string ProductIdsDoNotExistPrefix = "ProductId(s) do not exist:";

    public static string ProductIdsDoNotExist(IEnumerable<int> productIds)
    {
        return $"{ProductIdsDoNotExistPrefix} {string.Join(", ", productIds)}";
    }
}

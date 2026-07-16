using System.Linq.Expressions;
using OffersService.Models;

namespace OffersService.Extensions;

public static class QueryableExtensions
{
    public static IQueryable<Product> WhereIdIn(this IQueryable<Product> query, IEnumerable<int> ids)
    {
        return query.WherePropertyIn(product => product.Id, ids);
    }

    public static IQueryable<T> WherePropertyIn<T, TValue>(
        this IQueryable<T> query,
        Expression<Func<T, TValue>> propertySelector,
        IEnumerable<TValue> values)
    {
        var valueArray = values.Distinct().ToArray();
        if (valueArray.Length == 0)
        {
            return query.Where(_ => false);
        }

        var parameter = propertySelector.Parameters.Single();
        Expression? filter = null;

        foreach (var value in valueArray)
        {
            var equalsValue = Expression.Equal(
                propertySelector.Body,
                Expression.Constant(value, typeof(TValue)));

            filter = filter is null ? equalsValue : Expression.OrElse(filter, equalsValue);
        }

        return query.Where(Expression.Lambda<Func<T, bool>>(filter!, parameter));
    }
}

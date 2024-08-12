using System;
using System.Linq;
using System.Linq.Expressions;
using DynamicData;
using DynamicData.Binding;

namespace Gami.Desktop.MIsc;

public static class QueryableExt
{
    public static IQueryable<T> Sort<T, TF>(this IQueryable<T> queryable, Expression<Func<T, TF>> field, SortDirection sortDirection)
    {
        return sortDirection switch
        {
            SortDirection.Ascending => queryable.OrderBy(field),
            SortDirection.Descending => queryable.OrderByDescending(field),
            _ => throw new UnspecifiedIndexException(nameof(sortDirection))
        };
    }
}
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
        switch (sortDirection)
        {
            case SortDirection.Ascending: return queryable.OrderBy(field);
            case SortDirection.Descending: return queryable.OrderByDescending(field);
        }

        throw new UnspecifiedIndexException(nameof(sortDirection));
    }
}
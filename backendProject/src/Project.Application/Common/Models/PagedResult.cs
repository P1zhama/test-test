using System;
using System.Collections.Generic;

namespace Project.Application.Common.Models
{
    public class PagedResult<T>
    {
        public PagedResult(IReadOnlyList<T> items, long totalCount, int page, int pageSize)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }

        public IReadOnlyList<T> Items { get; }

        public long TotalCount { get; }

        public int Page { get; }

        public int PageSize { get; }

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}

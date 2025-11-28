using System;
using System.Collections.Generic;

namespace FinTech.Shared.Common
{
    /// <summary>
    /// Paginated result for list queries
    /// </summary>
    public class PaginatedResult<T>
    {
        public List<T> Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;

        public PaginatedResult(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public static PaginatedResult<T> Success(List<T> items, int pageNumber, int pageSize, int totalCount)
            => new(items, pageNumber, pageSize, totalCount);

        public static PaginatedResult<T> Empty()
            => new(new List<T>(), 1, 10, 0);
    }
}

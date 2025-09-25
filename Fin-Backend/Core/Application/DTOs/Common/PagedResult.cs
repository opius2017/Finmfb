using System.Collections.Generic;

namespace FinTech.Core.Application.DTOs.Common
{
    /// <summary>
    /// A paged result of items
    /// </summary>
    /// <typeparam name="T">Type of items</typeparam>
    public class PagedResult<T>
    {
        /// <summary>
        /// Items in the current page
        /// </summary>
        public List<T> Items { get; set; }

        /// <summary>
        /// Total number of items across all pages
        /// </summary>
        /// <example>100</example>
        public int TotalCount { get; set; }

        /// <summary>
        /// Current page number
        /// </summary>
        /// <example>1</example>
        public int PageNumber { get; set; }

        /// <summary>
        /// Number of items per page
        /// </summary>
        /// <example>20</example>
        public int PageSize { get; set; }

        /// <summary>
        /// Total number of pages
        /// </summary>
        /// <example>5</example>
        public int TotalPages { get; set; }
    }
}

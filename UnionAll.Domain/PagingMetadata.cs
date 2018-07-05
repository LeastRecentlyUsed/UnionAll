using System;
using System.Collections.Generic;
using System.Text;

namespace DataFork.Domain
{
    public class PagingMetadata<T>: List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItemCount { get; private set; }

        // is there a previous page?
        public bool HasPrevious
        {
            get { return (CurrentPage > 1); }
        }
        // is there a next page?
        public bool HasNext
        {
            get { return (CurrentPage < TotalPages); }
        }

        public PagingMetadata(List<T> items, int totalItemCount, int currentPageNumber, int pageSize)
        {
            // add the database reocrds.
            this.AddRange(items);
            // private set the properties in a constructor.
            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            CurrentPage = currentPageNumber;
            // e.g. if there are 101 records with a page size of 10, total pages will be 11.
            TotalPages = (int)Math.Ceiling(totalItemCount / (double)pageSize);
        }

    }
}

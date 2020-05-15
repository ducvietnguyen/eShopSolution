using System;
using System.Collections.Generic;
using System.Text;

namespace eShopSolution.ViewModel.Common
{
    public class PagedResultBase
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }

        public int FirstRowOnPage
        {
            get { return (PageIndex - 1) * PageSize + 1; }
        }

        public int LastRowOnPage
        {
            get { return Math.Min(PageIndex * PageSize, TotalRecords); }
        }

        public int PageCount
        {
            get
            {
                var pageCount = (double)TotalRecords / PageSize;
                return (int)Math.Ceiling(pageCount);
            }
        }
    }
}
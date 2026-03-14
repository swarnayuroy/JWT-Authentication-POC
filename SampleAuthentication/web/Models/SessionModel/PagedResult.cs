using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace web.Models.SessionModel
{
    public class PagedResult<T> where T : class
    {
        public IEnumerable<T> Items { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int ItemCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)ItemCount / PageSize); } }
    }
}
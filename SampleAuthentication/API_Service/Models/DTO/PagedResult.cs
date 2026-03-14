namespace API_Service.Models.DTO
{
    public class PagedResult<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int ItemCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)ItemCount / PageSize); } }
    }
}

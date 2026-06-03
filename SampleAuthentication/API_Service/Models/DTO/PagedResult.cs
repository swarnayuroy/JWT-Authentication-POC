namespace API_Service.Models.DTO
{
    //Grabs paginated user data and metadata
    public class PagedResult<T> where T : class
    {
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int ItemCount { get; set; }
        public int TotalPages { get { return (int)Math.Ceiling((double)ItemCount / PageSize); } }
    }

    public class AdminResult : PagedResult<UserDetail>
    {
        public int AdminCount { get; set; }
        public int SuperadminCount { get; set; }
        public IEnumerable<UserDetail> SuffixIdentifiedAdmin(string userId, List<UserDetail> admins)
        {
            admins[admins.IndexOf(admins.First(admin => admin.Id == userId))].Name = (from admin in admins 
                                                                                      where admin.Id == userId 
                                                                                      select admin.Name).FirstOrDefault() + " (You)";
            return admins;
        }
    }
}

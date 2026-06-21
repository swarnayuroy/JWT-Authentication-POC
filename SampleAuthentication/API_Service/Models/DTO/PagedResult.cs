using System.Linq;

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
        public IEnumerable<UserDetail> SuffixIdentifiedAdmin(string userId, IList<UserDetail> admins)
        {
            if (!string.IsNullOrEmpty(userId) && (admins != null && admins.Any()))
            {
                admins[admins.IndexOf(admins.First(admin => 
                Guid.Parse(admin.Id) == Guid.Parse(userId)))].Name = (from admin in admins
                                                                      where Guid.Parse(admin.Id) == Guid.Parse(userId)
                                                                      select admin.Name).FirstOrDefault() + " (You)";

                return admins;
            }

            return Enumerable.Empty<UserDetail>();
        }
    }
}

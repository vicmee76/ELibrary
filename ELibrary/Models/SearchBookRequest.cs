namespace ELibrary.Models
{
    public class SearchBookRequest : PagedRequest
    {
        public string SearchText { get; set; }
    }
    public class SearchBookByTopicRequest : PagedRequest
    {
        public string Topic { get; set; }
    }
}

namespace ELibrary.Models
{
    public class PagedRequest
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
    }
    public class PagedResponse<T>
    {
        public int PageSize { get; set; }
        public int Page { get; set; }
        public int TotalCount { get; set; }
        public required List<T> Items { get; set; }

    }
}

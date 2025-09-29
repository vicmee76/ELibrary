namespace ELibrary.Core.Models
{
    public class BookSummary
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
    }
}

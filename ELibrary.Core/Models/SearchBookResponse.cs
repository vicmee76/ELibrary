namespace ELibrary.Core.Models
{
    public class SearchBookResponse
    {
        public int Count { get; set; }
        public string NextPageUrl { get; set; }
        public string PreviousPageUrl { get; set; }
        public List<Data> Data { get; set; }
    }


    public class Data
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; }
        public string Summary { get; set; }
        public string ImageUrl { get; set; }
    }
}

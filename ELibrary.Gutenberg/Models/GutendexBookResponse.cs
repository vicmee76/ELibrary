namespace ELibrary.Infrastruture.Models
{
    public class GutendexBookResponse
    {
        public int count { get; set; }
        public object next { get; set; }
        public object previous { get; set; }
        public List<Result> results { get; set; }
    }

    public class Result
    {
        public int id { get; set; }
        public string title { get; set; }
        public List<Author> authors { get; set; }
        public List<string> summaries { get; set; }
        public List<object> translators { get; set; }
        public List<string> subjects { get; set; }
        public List<string> bookshelves { get; set; }
        public List<string> languages { get; set; }
        public bool copyright { get; set; }
        public string media_type { get; set; }
        public Formats formats { get; set; }
        public int download_count { get; set; }
    }

    public class Formats
    {
        public string texthtml { get; set; }
        public string applicationepubzip { get; set; }
        public string applicationxmobipocketebook { get; set; }
        public string textplaincharsetusascii { get; set; }
        public string applicationrdfxml { get; set; }
        public string imagejpeg { get; set; }
        public string applicationoctetstream { get; set; }
    }

    public class Author
    {
        public string name { get; set; }
        public int? birth_year { get; set; }
        public int? death_year { get; set; }
    }

}

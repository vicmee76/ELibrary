/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELibrary.Core.Entities
{
    public class Author
    {
        public string Name { get; set; }
        public int BirthYear { get; set; }
        public int DeathYear { get; set; }
    }

    public class Formats
    {
        [JsonProperty("text/html")]
        public string TextHtml { get; set; }

        [JsonProperty("application/epub+zip")]
        public string ApplicationEpubZip { get; set; }

        [JsonProperty("application/x-mobipocket-ebook")]
        public string ApplicationXMobipocketEbook { get; set; }

        [JsonProperty("text/plain; charsetus-ascii")]
        public string TextPlainCharsetusAscii { get; set; }

        [JsonProperty("application/rdf+xml")]
        public string ApplicationRdfXml { get; set; }

        [JsonProperty("image/jpeg")]
        public string ImageJpeg { get; set; }

        [JsonProperty("application/octet-stream")]
        public string ApplicationOctetStream { get; set; }
    }

    public class BookMeta
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public List<Author> Authors { get; set; }
        public List<string> Summaries { get; set; }
        public List<object> Translators { get; set; }
        public List<string> Subjects { get; set; }
        public List<string> Bookshelves { get; set; }
        public List<string> Languages { get; set; }
        public bool Copyright { get; set; }
        public string MediaType { get; set; }
        public Formats Formats { get; set; }
        public int DownloadCount { get; set; }
    }


}
*/
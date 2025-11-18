using System.Text.Json.Serialization;

namespace ELibrary.Gutenberg.Models
{
    // Google Books API DTOs
    public class GoogleBooksResponse
    {
        [JsonPropertyName("totalItems")]
        public int totalItems { get; set; }

        [JsonPropertyName("items")]
        public List<GoogleBookItem>? items { get; set; }
    }

    public class GoogleBookItem
    {
        [JsonPropertyName("id")]
        public string id { get; set; } = string.Empty;

        [JsonPropertyName("volumeInfo")]
        public VolumeInfo volumeInfo { get; set; } = new();

        [JsonPropertyName("accessInfo")]
        public AccessInfo? accessInfo { get; set; }
    }

    public class VolumeInfo
    {
        [JsonPropertyName("title")]
        public string? title { get; set; }

        [JsonPropertyName("authors")]
        public List<string>? authors { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("imageLinks")]
        public ImageLinks? imageLinks { get; set; }

        [JsonPropertyName("previewLink")]
        public string? previewLink { get; set; }

        [JsonPropertyName("infoLink")]
        public string? infoLink { get; set; }

        [JsonPropertyName("subtitle")]
        public string? subtitle { get; set; }
    }

    public class ImageLinks
    {
        [JsonPropertyName("thumbnail")]
        public string? thumbnail { get; set; }

        [JsonPropertyName("small")]
        public string? small { get; set; }

        [JsonPropertyName("medium")]
        public string? medium { get; set; }

        [JsonPropertyName("large")]
        public string? large { get; set; }
    }

    public class AccessInfo
    {
        [JsonPropertyName("webReaderLink")]
        public string? WebReaderLink { get; set; }

        [JsonPropertyName("embeddable")]
        public bool Embeddable { get; set; }

        [JsonPropertyName("viewability")]
        public string? Viewability { get; set; }
    }
}

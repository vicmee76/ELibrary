using ELibrary.Core.Enums;
using ELibrary.Core.Helpers;
using ELibrary.Core.Interfaces;
using ELibrary.Core.Models;
using ELibrary.Gutenberg.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Reflection.Metadata.BlobBuilder;

namespace ELibrary.Gutenberg.Services
{
    public class GoogleService : ILibraryService
    {
        private readonly IApiClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<GoogleService> _logger;
        public BookSource BOOK_SOURCE => BookSource.Google;

        public GoogleService(IApiClient client, IConfiguration config, ILogger<GoogleService> logger)
        {
            _client = client;
            _config = config;
            _logger = logger;
        }

        public async Task<Response<string>> GetBookById(string id)
        {
            try
            {
                var googleBooksBaseUrl = _config.GetValue<string>("GoogleBooks:BaseUrl");
                var apiKey = _config.GetValue<string>("GoogleBooks:ApiKey");

                var url = googleBooksBaseUrl + $"/volumes/{id}";

                if (!string.IsNullOrEmpty(apiKey))
                {
                    url += $"?key={apiKey}";
                }

                var book = await _client.GetAsync<GoogleBookItem>(url);

                if (book == null || !book.accessInfo.Embeddable /*|| !string.Equals(book.accessInfo?.Viewability, "ALL_PAGES", StringComparison.OrdinalIgnoreCase)*/)
                {
                    return new Response<string>(null, $"Book content not found for book {id}", false);
                }
                string bookHtml = GetBookViewerHtml(book.id, book.volumeInfo.title);
                //var bookContent = book.accessInfo?.webReaderLink ?? book.volumeInfo?.infoLink;

                return new Response<string>(bookHtml, "Book content URL retrieved successfully", true);
            }
            catch (Exception ex)
            {
                return new Response<string>(null, $"An error occurred getting book", false);
            }
        }

        public async Task<Response<BookSummary>> GetBookSummaryById(string id)
        {
            try
            {
                var googleBooksBaseUrl = _config.GetValue<string>("GoogleBooks:BaseUrl");
                var apiKey = _config.GetValue<string>("GoogleBooks:ApiKey");
                var eLibraryBaseUrl = _config.GetValue<string>("ElibraryBaseUrl");

                var url = googleBooksBaseUrl + $"/volumes/{id}";

                if (!string.IsNullOrEmpty(apiKey))
                {
                    url += $"?key={apiKey}";
                }

                var book = await _client.GetAsync<GoogleBookItem>(url);

                if (book?.volumeInfo == null)
                {
                    return new Response<BookSummary>(null, $"Book with id {id} not found", false);
                }

                var summary = new BookSummary
                {
                    Id = id,
                    Title = book.volumeInfo.title ?? "Unknown Title",
                    Authors = book.volumeInfo.authors ?? new List<string>(),
                    Summary = TruncateText(book.volumeInfo.description, 200) ?? "No Summary Available",
                    ImageUrl = eLibraryBaseUrl + $"/Image/{id}",
                    IsPartial = !string.Equals(book.accessInfo?.Viewability, "ALL_PAGES", StringComparison.OrdinalIgnoreCase),
                };

                return new Response<BookSummary>(summary, "Book summary retrieved successfully", true);
            }
            catch (Exception ex)
            {
                return new Response<BookSummary>(null, $"An error occurred getting book summary", false);
            }
        }

        public async Task<Response<FileContentResult>> GetImageById(string id)
        {
            try
            {
                var googleBooksBaseUrl = _config.GetValue<string>("GoogleBooks:BaseUrl");
                var apiKey = _config.GetValue<string>("GoogleBooks:ApiKey");

                var url = googleBooksBaseUrl + $"/volumes/{id}";

                if (!string.IsNullOrEmpty(apiKey))
                {
                    url += $"?key={apiKey}";
                }

                var book = await _client.GetAsync<GoogleBookItem>(url);
                var imageUrl = GetBestImageUrl(book?.volumeInfo?.imageLinks);

                if (string.IsNullOrEmpty(imageUrl))
                {
                    return new Response<FileContentResult>(null, $"Image not found for book {id}", false);
                }

                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(imageUrl);

                var fileContent = new FileContentResult(bytes, "image/jpeg")
                {
                    FileDownloadName = $"book_{id}.jpg"
                };

                return new Response<FileContentResult>(fileContent, "Image retrieved successfully", true);
            }
            catch (Exception ex)
            {
                return new Response<FileContentResult>(null, $"An error occurred getting image for book {id}", false);
            }
        }

        public async Task<Response<SearchBookResponse>> SearchBooks(int? page, string searchText)   
        {
            try
            {
                var googleBooksBaseUrl = _config.GetValue<string>("GoogleBooks:BaseUrl");
                var apiKey = _config.GetValue<string>("GoogleBooks:ApiKey");
                var eLibraryBaseUrl = _config.GetValue<string>("ElibraryBaseUrl");
                var maxResults = _config.GetValue<int>("GoogleBooks:MaxResults", 10);


                var startIndex = ((page ?? 1) - 1) * maxResults;
                if (startIndex < 0) startIndex = 0;
                var url = googleBooksBaseUrl + $"/volumes?q={Uri.EscapeDataString(searchText)}&startIndex={startIndex}&maxResults={maxResults}&filter=ebooks";

                if (!string.IsNullOrEmpty(apiKey))
                {
                    url += $"&key={apiKey}";
                }

                // _logger.LogInformation("Google Books API URL: {Url}", url);

                var books = await _client.GetAsync<GoogleBooksResponse>(url);

                var totalPages = (int)Math.Ceiling((double)(books?.totalItems ?? 0) / maxResults);
                var currentPage = page ?? 1;

                var response = new Response<SearchBookResponse>(
                    new SearchBookResponse
                    {
                        Count = books?.items?.Count(x => x.accessInfo.Embeddable /*&& string.Equals( x.accessInfo.Viewability, "ALL_PAGES",StringComparison.OrdinalIgnoreCase)*/) ?? 0,
                        NextPageUrl = currentPage < totalPages ? eLibraryBaseUrl + $"/Search?Page={currentPage + 1}&SearchText={searchText}" : null,
                        PreviousPageUrl = currentPage > 1 ? eLibraryBaseUrl + $"/Search?Page={currentPage - 1}&SearchText={searchText}" : null,
                        Data = books?.items?.Where(b => b.accessInfo.Embeddable /*&& string.Equals(b.accessInfo.Viewability, "ALL_PAGES", StringComparison.OrdinalIgnoreCase)*/).Select(b => new Data
                        {
                            Id = b.id,
                            Title = b.volumeInfo?.title ?? "Unknown Title",
                            Authors = b.volumeInfo?.authors ?? new List<string>(),
                            Summary = TruncateText(b.volumeInfo?.subtitle, 200) ?? "No Summary Available",
                            ImageUrl = eLibraryBaseUrl + $"/Image/{b.id}",
                            Source = BOOK_SOURCE,
                            IsPartial = !string.Equals(b.accessInfo.Viewability, "ALL_PAGES", StringComparison.OrdinalIgnoreCase)
                        }).ToList() ?? new List<Data>()
                    },
                    "Books retrieved successfully",
                    true
                );
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FATAL ERROR] SearchBooks failed for search: {SearchText}", searchText);
                return new Response<SearchBookResponse>(null, "An error occurred getting books", false);
            }
        }

        public async Task<Response<SearchBookResponse>> SearchBooksByTopic(int? page, string topic)
        {
            try
            {
                var googleBooksBaseUrl = _config.GetValue<string>("GoogleBooks:BaseUrl");
                var apiKey = _config.GetValue<string>("GoogleBooks:ApiKey");
                var eLibraryBaseUrl = _config.GetValue<string>("ElibraryBaseUrl");
                var maxResults = _config.GetValue<int>("GoogleBooks:MaxResults", 10);


                var startIndex = ((page ?? 1) - 1) * maxResults;
                if (startIndex < 0) startIndex = 0;
                var url = googleBooksBaseUrl + $"/volumes?q=subject:{Uri.EscapeDataString(topic)}&startIndex={startIndex}&maxResults={maxResults}&filter=ebooks";

                if (!string.IsNullOrEmpty(apiKey))
                {
                    url += $"&key={apiKey}";
                }

                // _logger.LogInformation("Google Books API URL: {Url}", url);

                var books = await _client.GetAsync<GoogleBooksResponse>(url);

                var totalPages = (int)Math.Ceiling((double)(books?.totalItems ?? 0) / maxResults);
                var currentPage = page ?? 1;

                var response = new Response<SearchBookResponse>(
                    new SearchBookResponse
                    {
                        Count = books?.items?.Count(x => x.accessInfo.Embeddable /*&& string.Equals(x.accessInfo.Viewability, "ALL_PAGES", StringComparison.OrdinalIgnoreCase)*/) ?? 0,
                        NextPageUrl = currentPage < totalPages ? eLibraryBaseUrl + $"/SearchTopic?Page={currentPage + 1}&Topic={topic}" : null,
                        PreviousPageUrl = currentPage > 1 ? eLibraryBaseUrl + $"/SearchTopic?Page={currentPage - 1}&Topic={topic}" : null,
                      
                        Data = books?.items?.Where(b => b.accessInfo.Embeddable /*&& string.Equals(b.accessInfo.Viewability, "ALL_PAGES", StringComparison.OrdinalIgnoreCase)*/).Select(b => new Data
                        {
                            Id = b.id,
                            Title = b.volumeInfo?.title ?? "Unknown Title",
                            Authors = b.volumeInfo?.authors ?? new List<string>(),
                            Summary = TruncateText(b.volumeInfo?.description, 200) ?? "No Summary Available",
                            ImageUrl = eLibraryBaseUrl + $"/Image/{b.id}",
                            Source = BOOK_SOURCE,
                            IsPartial = !string.Equals(b.accessInfo.Viewability, "ALL_PAGES", StringComparison.OrdinalIgnoreCase)
                        }).ToList() ?? new List<Data>()
                    },
                    "Books retrieved successfully",
                    true
                );
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[FATAL ERROR] SearchBooks failed for topic: {Topic}", topic);
                return new Response<SearchBookResponse>(null, "An error occurred getting books", false);
            }
        }

        //private static bool IsBookViewable(GoogleBookItem book)
        //{
        //    if (book?.accessInfo == null)
        //        return false;

        //    // Only return books that are embeddable
        //    return book.accessInfo.embeddable;
        //}


        private static string GetBookViewerHtml(string volumeId, string bookTitle)
        {
            var template = @"<!DOCTYPE html ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <meta http-equiv=""content-type"" content=""text/html; charset=utf-8"" />
    <title>{{Book Title Here}}</title>
    <script type=""text/javascript"" src=""https://www.google.com/books/jsapi.js""></script>
    <script type=""text/javascript"">
        google.books.load();
        function alertNotFound() {
            alert(""could not embed the book!"");
        }
        function initialize() {
            var viewer = new google.books.DefaultViewer(document.getElementById('viewerCanvas'));
            viewer.load('[[IDENTIFY]]', alertNotFound);
        }
        google.books.setOnLoadCallback(initialize);
    </script>
    <style>
   #viewerCanvas>div {
       border: none !important;
       box-shadow: none !important;
   }
   #viewerCanvas>div>div:nth-last-child(1) {
       display: none;
   }
   #viewerCanvas>div>div:nth-child(1)>div:nth-child(2) {
       display: none;
   }
   /* #viewerCanvas>div>div:nth-child(1)>div:nth-child(1) {
       width: 100% !important;
       height: 100vh !important;
   } */
</style>
</head>
<body>
    <div id=""viewerCanvas"" style=""width: 100%; min-height: 100%""></div>
</body>
</html>";

            return template.Replace("[[IDENTIFY]]", volumeId)
                          .Replace("{{Book Title Here}}", bookTitle);
        }

        private static string? GetBestImageUrl(ImageLinks? imageLinks)
        {
            if (imageLinks == null) return null;

            // Prefer larger images, fallback to smaller ones
            var imageUrl = imageLinks.large ??
                          imageLinks.medium ??
                          imageLinks.small ??
                          imageLinks.thumbnail;

            // Ensure HTTPS
            if (!string.IsNullOrEmpty(imageUrl) && imageUrl.StartsWith("http://"))
            {
                imageUrl = imageUrl.Replace("http://", "https://");
            }

            return imageUrl;
        }

        private static string TruncateText(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            if (text.Length <= maxLength) return text;

            var truncated = text.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');
            if (lastSpace > 0)
            {
                truncated = truncated.Substring(0, lastSpace);
            }
            return truncated + "...";
        }
    }
}

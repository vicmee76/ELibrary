using ELibrary.Core.Constants;
using ELibrary.Core.Enums;
using ELibrary.Core.Helpers;
using ELibrary.Core.Interfaces;
using ELibrary.Core.Models;
using ELibrary.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ELibrary.Infrastructure.Services
{
    public class DoaBooksService : ILibraryService
    {
        private readonly IApiClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<DoaBooksService> _logger;

        private readonly string _doaBaseUrl = string.Empty;
        private readonly string _eLibraryBaseUrl = string.Empty;
        private readonly string _imageUrl = string.Empty;

        public BookSource BOOK_SOURCE => BookSource.DOA;


        public DoaBooksService(IApiClient client, IConfiguration config, ILogger<DoaBooksService> logger)
        {
            _config = config;
            _client = client;
            _logger = logger;

            _doaBaseUrl = _config.GetValue<string>("DoaBooks:BaseUrl") ?? string.Empty;
            _imageUrl = _config.GetValue<string>("DoaBooks:ImageUrl") ?? string.Empty;
            _eLibraryBaseUrl = _config.GetValue<string>("ElibraryBaseUrl") ?? string.Empty;
        }



        public async Task<Response<string>> GetBookById(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                    return new Response<string>(null, $"Book with ID {id} not found", false);
                
                var book = await GetBookSummaryById(id);
                return book.Data.viewLink == null
                    ? new Response<string>(null, $"Book content not found for book {id}", false)
                    : new Response<string>(book.Data.viewLink);
            }
            catch (Exception e)
            {
                return new Response<string>(null, $"An error occurred getting book", false);
            }
        }



        public async Task<Response<BookSummary>> GetBookSummaryById(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                    return new Response<BookSummary>(null, $"Book with ID {id} not found", false);
                
                _logger.LogInformation($"DoaBooksService[GetBookSummaryById] : Getting book content for book {id}");
                
                var url = $"{_doaBaseUrl}/rest/items/{id}?expand=all";
                var book = await _client.GetAsync<DoaBookResponseModel>(url);

                _logger.LogInformation(
                    $"DoaBooksService[GetBookSummaryById] : Response from api : {JsonConvert.SerializeObject(book)}");
                
                var pdfLink = string.Empty;
                
                foreach (var bit in book.Bitstreams.Where(bit => bit.Metadata.Any(x => x.Key == Constants.DownloadKey)))
                {
                    pdfLink = bit.Metadata.Where(x => x.Key == Constants.DownloadKey)?.FirstOrDefault()?.Value.ToString();
                    break;
                }

                if (string.IsNullOrEmpty(pdfLink))
                    return new Response<BookSummary>(null, $"Book with id {id} not found", false);
                
                var summary = new BookSummary
                {
                    Id = id,
                    Title = book.Name ?? "Unknown Title",
                    Authors = book.Metadata
                        .Where(m => m.Key == Constants.AuthorKey || (m.Key == Constants.EditorKey && !book.Metadata.Any(x => x.Key == Constants.AuthorKey)))
                        .Where(m => m.Key == Constants.AuthorKey || m.Key == Constants.EditorKey)
                        .Select(m => m.Value?.ToString() ?? string.Empty)
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList(),
                    Summary = Util.TruncateText(book.Metadata.FirstOrDefault(x => x.Key == Constants.DescriptionKey)?.Value?.ToString(), 200) ?? "No Summary Available",
                    ImageUrl = _eLibraryBaseUrl + $"/Image/{book.Uuid}",
                    viewLink = ViewBook(pdfLink),
                };

                _logger.LogInformation(
                    $"DoaBooksService[GetBookSummaryById] : Response from api : {JsonConvert.SerializeObject(summary)}");
                
                return new Response<BookSummary>(summary, "Book summary retrieved successfully", true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DoaBooksService[GetBookSummaryById] : Something went wrong");
                return new Response<BookSummary>(null, $"An error occurred getting book summary", false);
            }
        }



        public async Task<Response<FileContentResult>> GetImageById(string id)
        {
            try
            {
                if(string.IsNullOrEmpty(id))
                    return new Response<FileContentResult>(null, $"Book with ID {id} not found", false);
                
                _logger.LogInformation(
                    $"DoaBooksService[GetImageById] : Getting image content for book {id}");
                
                var imageUrl = $"{_imageUrl}/{id}/retrieve";
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
                _logger.LogError(ex, "DoaBooksService[GetImageById] : Something went wrong");
                return new Response<FileContentResult>(null, "Something went wrong.", false);
            }
        }




        public async Task<Response<SearchBookResponse>> SearchBooks(int? page, string searchText)
        {
            try
            {
                if(string.IsNullOrEmpty(searchText))
                    return new Response<SearchBookResponse>(null, $"Book search text not found", false);
                
                var split = searchText.Split(' ');
                var query = string.Join("+", split);
                var url = $"{_doaBaseUrl}/rest/search?query=dc.title:{query}&expand=metadata";

                _logger.LogInformation(
                    $"DoaBooksService[SearchBooks] : About to get book search result with url: {url}");

                var books = await _client.GetAsync<List<DoaBookResults>>(url);
                var response = new Response<SearchBookResponse>(
                    new SearchBookResponse
                    {
                        Count = books.Count,
                        NextPageUrl = string.Empty,
                        PreviousPageUrl = string.Empty,
                        Data = books.Select(b => new Data
                        {
                            Id = b.Uuid,
                            Title = b.Name,
                            Authors = b.Metadata
                                .Where(m => m.Key == Constants.AuthorKey || (m.Key == Constants.EditorKey &&
                                                                             !b.Metadata.Any(x =>
                                                                                 x.Key == Constants.AuthorKey)))
                                .Where(m => m.Key == Constants.AuthorKey || m.Key == Constants.EditorKey)
                                .Select(m => m.Value?.ToString() ?? string.Empty)
                                .Where(s => !string.IsNullOrEmpty(s))
                                .ToList(),

                            Summary = b.Metadata.FirstOrDefault(x => x.Key == Constants.DescriptionKey)?.Value
                                ?.ToString() ?? string.Empty,
                            ImageUrl = _eLibraryBaseUrl + $"/Image/{b.Uuid}",
                            Source = BOOK_SOURCE,
                        }).ToList() ?? new List<Data>()
                    },
                    "Books retrieved successfully",
                    true
                );

                _logger.LogInformation(
                    $"DoaBooksService[SearchBooks] : Books retrieved successfully with search text: {searchText} and count {response.Data.Count}");
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DoaBooksService[SearchBooks] : Something went wrong");
                return new Response<SearchBookResponse>(null, "Something went wrong.", false);
            }
        }



        public async Task<Response<SearchBookResponse>> SearchBooksByTopic(int? page, string topic)
        {
            return await SearchBooks(page, topic);
        }



        private string ViewBook(string? pdfUrl)
        {
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>PDF Viewer</title>
                </head>
                <body style='margin:0;padding:0;'>
                    <iframe 
                        src='{pdfUrl}' 
                        style='width:100%;height:100vh;border:none;'>
                    </iframe>
                </body>
                </html>
            ";
            return html;
        }

    }
}

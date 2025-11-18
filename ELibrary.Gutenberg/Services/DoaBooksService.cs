using ELibrary.Core.Enums;
using ELibrary.Core.Helpers;
using ELibrary.Core.Interfaces;
using ELibrary.Core.Models;
using ELibrary.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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



        public Task<Response<string>> GetBookById(string id)
        {
            throw new NotImplementedException();
        }



        public Task<Response<BookSummary>> GetBookSummaryById(string id)
        {
            throw new NotImplementedException();
        }



        public async Task<Response<FileContentResult>> GetImageById(string id)
        {
           var imageUrl = $"{_imageUrl}/{id}/retrieve";

            using var client = new HttpClient();
            var bytes = await client.GetByteArrayAsync(imageUrl);

            var fileContent = new FileContentResult(bytes, "image/jpeg")
            {
                FileDownloadName = $"book_{id}.jpg"
            };

            return new Response<FileContentResult>(fileContent, "Image retrieved successfully", true);
        }




        public async Task<Response<SearchBookResponse>> SearchBooks(int? page, string searchText)
        {
            var split = searchText.Split(' ');
            var query = string.Join("+", split);
            var url = $"{_doaBaseUrl}/rest/search?query=dc.title:{query}&expand=metadata,bitstreams";

            var books = await _client.GetAsync<DoaSearchResponseModel>(url);
            var response = new Response<SearchBookResponse>(
                  new SearchBookResponse
                  {
                      Count = books.doaResults.Count,
                      NextPageUrl = string.Empty,
                      PreviousPageUrl = string.Empty,
                      Data = books?.doaResults?.Select(b => new Data
                      {
                          Id = b.Uuid,
                          Title = b.Name,
                          Authors = b.Metadata
                                    .Where(m => m.Key == "dc.contributor.author" || (m.Key == "dc.contributor.editor" && !b.Metadata.Any(x => x.Key == "dc.contributor.author")))
                                    .Where(m => m.Key == "dc.contributor.author" || m.Key == "dc.contributor.editor")
                                    .Select(m => m.Value?.ToString() ?? string.Empty)
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .ToList(),

                          Summary = b.Metadata.FirstOrDefault(x => x.Key == "dc.description.abstract")?.Value?.ToString() ?? string.Empty,
                          ImageUrl = _eLibraryBaseUrl + $"/Image/{b.Uuid}",
                          Source = BOOK_SOURCE,
                      }).ToList() ?? new List<Data>()
                  },
                  "Books retrieved successfully",
                  true
              );
            return response;
        }



        public Task<Response<SearchBookResponse>> SearchBooksByTopic(int? page, string topic)
        {
            throw new NotImplementedException();
        }

    }
}

﻿using ELibrary.Core.Enums;
using ELibrary.Core.Helpers;
using ELibrary.Core.Interfaces;
using ELibrary.Core.Models;
using ELibrary.Infrastruture.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELibrary.Infrastruture.Services
{
    public class GutendexService : ILibraryService
    {
        private readonly IApiClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<GutendexService> _logger;
        public BookSource BOOK_SOURCE => BookSource.Gutenberg;

        public GutendexService(IApiClient client, IConfiguration config, ILogger<GutendexService> logger)
        {
            _client = client;
            _config = config;
            _logger = logger;
        }

        public async Task<Response<string>> GetBookById(string id)
        {
            try
            {
                var gutendexBaseUrl = _config.GetValue<string>("Gutendex:BaseUrlForBooksById");

                // Gutendex single book endpoint
                //var url = gutendexBaseUrl + $"?ids={id}";
                //var book = await _client.GetAsync<GutendexBookResponse>(url);

                //if (book.results == null || book.results.FirstOrDefault().formats == null || string.IsNullOrEmpty(book.results.FirstOrDefault().formats.texthtml))
                //{
                //    return new Response<string>(null, $"Book content not found for book {id}", false);
                //}

                var bookHtml = await _client.GetAsync<string>(gutendexBaseUrl + $"{id}/pg{id}-images.html");

                if (string.IsNullOrEmpty(bookHtml))
                {
                    return new Response<string>(null, $"Book content not found for book {id}", false);
                }

                bookHtml = RemoveGutenbergBoilerplate(bookHtml);
                bookHtml = RemoveGutenbergBoilerplate(bookHtml);

                return new Response<string>(bookHtml, "Book HTML URL retrieved successfully", true);
            }
            catch (Exception ex)
            {
                return new Response<string>(null, $"An error occurred getting book ", false);
            }
        }

        public async Task<Response<BookSummary>> GetBookSummaryById(string id)
        {
            try
            {
                var gutendexBaseUrl = _config.GetValue<string>("Gutendex:BaseUrl");
                var eLibraryBaseUrl = _config.GetValue<string>("ElibraryBaseUrl");

                var url = gutendexBaseUrl + $"?ids={id}";
                var book = await _client.GetAsync<GutendexBookResponse>(url);

                if (book.results == null)
                {
                    return new Response<BookSummary>(null, $"Book with id {id} not found", false);
                }

                var summary = new BookSummary
                {
                    Id = book.results.FirstOrDefault().id.ToString(),
                    Title = book.results.FirstOrDefault().title,
                    Authors = book.results.FirstOrDefault().authors?.Select(a => a.name).ToList() ?? new List<string>(),
                    Summary = book.results.FirstOrDefault().summaries?.FirstOrDefault() ?? "No Summary Available",
                    ImageUrl = eLibraryBaseUrl + $"/Image/{book.results.FirstOrDefault().id}"
                };

                return new Response<BookSummary>(summary, "Book summary retrieved successfully", true);
            }
            catch (Exception ex)
            {
                // log exception with Serilog
                return new Response<BookSummary>(null, $"An error occurred getting book summary", false);
            }
        }


        public async Task<Response<FileContentResult>> GetImageById(string id)
        {
            try
            {
                var gutendexImageUrl = _config.GetValue<string>("Gutendex:ImageUrl");
                var url = gutendexImageUrl + $"{id}/pg{id}.cover.medium.jpg";

                using var client = new HttpClient();
                var bytes = await client.GetByteArrayAsync(url);

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
                var gutendexBaseUrl = _config.GetValue<string>("Gutendex:BaseUrl");
                var eLibraryBaseUrl = _config.GetValue<string>("ElibraryBaseUrl");
                var books = await _client.GetAsync<GutendexBooksResponse>(gutendexBaseUrl + $"?search={searchText}" + ((page.HasValue && page != 0)? $"&page={page}":""));

                var response = new Response<SearchBookResponse>(
                    new SearchBookResponse
                    {
                        Count = books.count,
                        NextPageUrl = books.next != null ? eLibraryBaseUrl + $"/Search?Page={page + 1}&SearchText={searchText}" : null,
                        PreviousPageUrl = books.previous != null ? eLibraryBaseUrl + $"/Search?Page={page - 1}&SearchText={searchText}" : null,
                        Data = books.results.Select(b => new Data
                        {
                            Id = b.id.ToString(),
                            Title = b.title,
                            Authors = b.authors != null ? b.authors.Select(a => a.name).ToList() : new List<string>(),                         
                            Source = BOOK_SOURCE,
                            Summary = b.summaries != null ? b.summaries?.FirstOrDefault() : "No Summary Available",
                            ImageUrl = eLibraryBaseUrl + $"/Image/{b.id}"
                        }).ToList()
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
                var gutendexBaseUrl = _config.GetValue<string>("Gutendex:BaseUrl");
                var eLibraryBaseUrl = _config.GetValue<string>("ElibraryBaseUrl");
                var books = await _client.GetAsync<GutendexBooksResponse>(gutendexBaseUrl + $"?topic={topic}" + ((page.HasValue && page != 0) ? $"&page={page}" : ""));

                var response = new Response<SearchBookResponse>(
                    new SearchBookResponse
                    {
                        Count = books.count,
                        NextPageUrl = books.next != null ? eLibraryBaseUrl + $"/SearchTopic?Page={page + 1}&Topic={topic}" : null,
                        PreviousPageUrl = books.previous != null ? eLibraryBaseUrl + $"/SearchTopic?Page={page - 1}&Topic={topic}" : null,
                        Data = books.results.Select(b => new Data
                        {
                            Id = b.id.ToString(),
                            Title = b.title,
                            Authors = b.authors != null ? b.authors.Select(a => a.name).ToList() : new List<string>(),                        
                            Source = BOOK_SOURCE,
                            Summary = b.summaries != null ? b.summaries?.FirstOrDefault() : "No Summary Available",
                            ImageUrl = eLibraryBaseUrl + $"/Image/{b.id}"
                        }).ToList()
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

        public static string RemoveGutenbergBoilerplate(string bookHtml)
        {
            if (string.IsNullOrWhiteSpace(bookHtml))
                return bookHtml;

            const string startTag = "<section class=\"pg-boilerplate pgheader\"";
            const string endTag = "</section>";

            int startIndex = bookHtml.IndexOf(startTag, StringComparison.OrdinalIgnoreCase);
            if (startIndex >= 0)
            {
                int endIndex = bookHtml.IndexOf(endTag, startIndex, StringComparison.OrdinalIgnoreCase);
                if (endIndex > startIndex)
                {
                    string toRemove = bookHtml.Substring(startIndex, endIndex - startIndex + endTag.Length);
                    bookHtml = bookHtml.Replace(toRemove, "");
                }
            }

            return bookHtml;
        }

        //public static int GetPageNumber(string url)
        //{
        //    if (string.IsNullOrEmpty(url))
        //        return -1;

        //    int pageIndex = url.IndexOf("page=", StringComparison.OrdinalIgnoreCase);
        //    if (pageIndex == -1)
        //        return -1;

        //    pageIndex += "page=".Length;

        //    int endIndex = url.IndexOf('&', pageIndex);
        //    string pageValue = (endIndex == -1)
        //        ? url.Substring(pageIndex)               
        //        : url.Substring(pageIndex, endIndex - pageIndex);

        //    return int.TryParse(pageValue, out int pageNumber) ? pageNumber : -1;
        //}
    }
}

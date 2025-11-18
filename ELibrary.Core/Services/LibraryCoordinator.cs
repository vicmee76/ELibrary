using ELibrary.Core.Enums;
using ELibrary.Core.Interfaces;
using ELibrary.Core.Models;
using Microsoft.AspNetCore.Mvc;
using static ELibrary.Core.Helpers.Util;

namespace ELibrary.Core.Services
{
    public class LibraryCoordinator
    {
        private readonly IEnumerable<ILibraryService> _libraryServices;


        public LibraryCoordinator(IEnumerable<ILibraryService> libraryServices)
        {
                _libraryServices = libraryServices;
        }



        public async Task<Response<SearchBookResponse>> SearchBooks(int? page, string searchText)
        {
            var libraryProviders = _libraryServices.ToArray();
            var tasks = new Task<Response<SearchBookResponse>>[libraryProviders.Length];
            for (int i = 0; i < _libraryServices.Count(); i++)
            {
                tasks[i] = libraryProviders[i].SearchBooks(page, searchText);
            }

            var results = await Task.WhenAll(tasks);
            return CombineResultsFromMultipleSources(results);
        }

        private Response<SearchBookResponse> CombineResultsFromMultipleSources(Response<SearchBookResponse>[] results)
        {
            return new Response<SearchBookResponse>(
               new SearchBookResponse
               {
                   Count = results.Where(s => s.Success).Sum(r => r.Data?.Count ?? 0),
                   Data = RemoveDuplicates(results.Where(r => r.Success).SelectMany(r => r.Data?.Data ?? new List<Data>()).OrderByDescending(x => x.Source).ToList())
               },
                results.Any(x => x.Success) ? "Books retrieved successfully from multiple sources" : string.Join(',', results.Where(s => !string.IsNullOrEmpty(s.Message)).Select(s => s.Message) ?? []),
               results.Any(x => x.Success));
        }
        
        
        private List<Data> RemoveDuplicates(List<Data> combinedData)
        {
            var uniqueData = new Dictionary<string, Data>(StringComparer.OrdinalIgnoreCase);
            foreach (var item in combinedData)
            {
                var key = $"{item.Title.SanitizeForComparison()}-{string.Join(",", item.Authors.OrderBy(a => a).Select(s => s.SanitizeForComparison(true)))}";
                if (!uniqueData.ContainsKey(key))
                {
                    uniqueData[key] = item;
                }
            }
            return uniqueData.Values.ToList();
        }
        
        
        public async Task<Response<SearchBookResponse>> SearchBooksByTopic(int? page, string topic)
        {
            var libraryProviders = _libraryServices.ToArray();
            var tasks = new Task<Response<SearchBookResponse>>[libraryProviders.Length];
            for (int i = 0; i < _libraryServices.Count(); i++)
            {
                tasks[i] = libraryProviders[i].SearchBooksByTopic(page, topic);
            }

            var results = await Task.WhenAll(tasks);
            return CombineResultsFromMultipleSources(results);
        }
        
        
        public async Task<Response<BookSummary>> GetBookSummaryById(BookSource source, string id)
        {
            return await _libraryServices.First(s => s.BOOK_SOURCE == source).GetBookSummaryById(id);
        }
        
        
        public async Task<Response<string>> GetBookById(BookSource source, string id)
        {
            return await _libraryServices.First(s => s.BOOK_SOURCE == source).GetBookById(id);
        }
        
        
        public async Task<Response<FileContentResult>> GetImageById(BookSource source, string id)
        {
            return await _libraryServices.First(s => s.BOOK_SOURCE == source).GetImageById(id);
        }
    }
}

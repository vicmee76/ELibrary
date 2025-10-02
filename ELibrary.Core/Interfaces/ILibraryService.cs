using ELibrary.Core.Enums;
using ELibrary.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Core.Interfaces
{
    public interface ILibraryService
    {
        BookSource BOOK_SOURCE { get; }
        Task<Response<SearchBookResponse>> SearchBooks(int? page, string searchText);
        Task<Response<SearchBookResponse>> SearchBooksByTopic(int? page, string topic);
        Task<Response<BookSummary>> GetBookSummaryById(long id);
        Task<Response<string>> GetBookById(long id);
        Task<Response<FileContentResult>> GetImageById(long id);
    }
}

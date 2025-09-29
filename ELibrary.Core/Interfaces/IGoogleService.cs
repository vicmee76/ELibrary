using ELibrary.Core.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELibrary.Core.Interfaces
{
    public interface IGoogleService
    {
        Task<Response<SearchBookResponse>> SearchBooks(int? page, string searchText);
        Task<Response<SearchBookResponse>> SearchBooksByTopic(int? page, string topic);
        Task<Response<BookSummary>> GetBookSummaryById(string id);
        Task<Response<string>> GetBookById(string id);
        Task<Response<FileContentResult>> GetImageById(string id);
    }
}

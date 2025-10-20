using ELibrary.Core.Enums;
using ELibrary.Core.Interfaces;
using ELibrary.Core.Models;
using ELibrary.Core.Services;
using ELibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class LibraryController : ControllerBase
    {
       
        private readonly ILogger<LibraryController> _logger;
        private readonly LibraryCoordinator _libraryCoordinator;
        public LibraryController(ILogger<LibraryController> logger, LibraryCoordinator libraryCoordinator)
        {
            _logger = logger;
            _libraryCoordinator = libraryCoordinator;
        }

        [HttpGet(Name = "search")]
        public async Task<ActionResult<SearchBookResponse>> Search([FromQuery] SearchBookRequest request)
        {
            try
            {
                var resp = await _libraryCoordinator.SearchBooks(request.Page, request.SearchText);
                if (resp.Success)
                {
                    return resp.Data;   
                }
                else
                {
                    return BadRequest(resp.Message);
                }
            }
            catch (Exception ex)
            {
                return  BadRequest(ex.Message);
            }
        }
        [HttpGet(Name = "searchTopic")]
        public async Task<ActionResult<SearchBookResponse>> SearchTopic([FromQuery] SearchBookByTopicRequest request)
        {
            try
            {
                var resp = await _libraryCoordinator.SearchBooksByTopic(request.Page, request.Topic);
                if (resp.Success)
                {
                    return resp.Data;
                }
                else
                {
                    return BadRequest(resp.Message);
                }
            }
            catch (Exception ex)
            {
                return  BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}",Name = "summary")]
        public async Task<ActionResult<BookSummary>> Summary(string id, BookSource source)
        {
            try
            {
                var resp = await _libraryCoordinator.GetBookSummaryById(source, id);
                if (resp.Success)
                {
                    resp.Data.Source = source;
                    return resp.Data;
                }
                else
                {
                    return BadRequest(resp.Message);
                }
            }
            catch (Exception ex)
            {
                return  BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}",Name = "book")]
        public async Task<ActionResult<string>> Book(string id, BookSource source)
        {
            try
            {
                var resp = await _libraryCoordinator.GetBookById(source, id);
                if (resp.Success)
                {
                    return resp.Data;
                }
                else
                {
                    return BadRequest(resp.Message);
                }
            }
            catch (Exception ex)
            {
                return  BadRequest(ex.Message);
            }
        }
        [HttpGet("{id}",Name = "image")]
        public async Task<ActionResult> Image(string id, BookSource source)
        {
            try
            {
                var resp = await _libraryCoordinator.GetImageById(source, id);
                if (resp.Success)
                {
                    return resp.Data;
                }
                else
                {
                    return BadRequest(resp.Message);
                }
            }
            catch (Exception ex)
            {
                return  BadRequest(ex.Message);
            }
        }
    }
}

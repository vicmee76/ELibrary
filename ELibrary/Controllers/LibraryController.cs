using ELibrary.Core.Interfaces;
using ELibrary.Core.Models;
using ELibrary.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELibrary.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("[controller]/[action]")]
    public class LibraryController : ControllerBase
    {
       
        private readonly ILogger<LibraryController> _logger;
        private readonly IGutendexService _libraryService;
        public LibraryController(ILogger<LibraryController> logger, IGutendexService libraryService)
        {
            _logger = logger;
            _libraryService = libraryService;
        }

        [HttpGet(Name = "search")]
        public async Task<ActionResult<SearchBookResponse>> Search([FromQuery] SearchBookRequest request)
        {
            try
            {
                var resp = await _libraryService.SearchBooks(request.Page, request.SearchText);
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
                var resp = await _libraryService.SearchBooksByTopic(request.Page, request.Topic);
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
        public async Task<ActionResult<BookSummary>> Summary(string id)
        {
            try
            {
                var resp = await _libraryService.GetBookSummaryById(id);
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
        [HttpGet("{id}",Name = "book")]
        public async Task<ActionResult<string>> Book(string id)
        {
            try
            {
                var resp = await _libraryService.GetBookById(id);
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
        public async Task<ActionResult> Image(string id)
        {
            try
            {
                var resp = await _libraryService.GetImageById(id);
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

using HackerRank.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerRank.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class HackerRankController : ControllerBase
    {
        private readonly IHackerRankService _hackerRankService;

        public HackerRankController(IHackerRankService hackerRankService)
        {
            _hackerRankService = hackerRankService;
        }

        [HttpGet]
        public async Task<IActionResult> GetNew()
        {
            var results = await _hackerRankService.GetNew();
            return Ok(results);
        }

        [HttpGet]
        public async Task<IActionResult> GetNewWithUrl()
        {
            var results = await _hackerRankService.GetNewWithUrl();
            return Ok(results);
        }


        [HttpGet]
        public async Task<IActionResult> Search(string? searchTerm, int page, int pageSize)
        {
            var results = await _hackerRankService.Search(searchTerm, page, pageSize);
            return Ok(results);
        }
    }
}

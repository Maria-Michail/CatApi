using CatApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatApi.Controllers
{
    [Route("api/cats")]
    [ApiController]
    public class CatsController : ControllerBase
    {
        private readonly ICatService _catService;

        public CatsController(ICatService catService)
        {
            _catService = catService;
        }

        [HttpPost("fetch")]
        public async Task<IActionResult> FetchCats()
        {
            var result = await _catService.FetchCatsAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCatById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID.");
            }

            var cat = await _catService.GetCatByIdAsync(id);
            if (cat == null)
            {
                return NotFound();
            }
            return Ok(cat);
        }

        [HttpGet]
        public async Task<IActionResult> GetCats([FromQuery] string? tag, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                return BadRequest("Invalid pagination parameters.");
            }

            var (totalCount, cats) = await _catService.GetCatsAsync(tag, page, pageSize);
            return Ok(new { totalCount, cats });
        }
    }
}

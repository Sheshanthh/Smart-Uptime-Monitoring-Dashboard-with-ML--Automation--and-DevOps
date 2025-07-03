using Microsoft.AspNetCore.Mvc;

namespace SmartUptime.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SitesController : ControllerBase
    {
        private static readonly List<Site> Sites = new();
        private static int _nextId = 1;

        [HttpGet]
        public ActionResult<IEnumerable<Site>> GetAll() => Ok(Sites);

        [HttpGet("{id}")]
        public ActionResult<Site> Get(int id)
        {
            var site = Sites.FirstOrDefault(s => s.Id == id);
            return site is not null ? Ok(site) : NotFound();
        }

        [HttpPost]
        public ActionResult<Site> Create([FromBody] SiteDto dto)
        {
            var site = new Site { Id = _nextId++, Url = dto.Url, Name = dto.Name };
            Sites.Add(site);
            return CreatedAtAction(nameof(Get), new { id = site.Id }, site);
        }

        [HttpPut("{id}")]
        public ActionResult<Site> Update(int id, [FromBody] SiteDto dto)
        {
            var site = Sites.FirstOrDefault(s => s.Id == id);
            if (site is null) return NotFound();
            site.Url = dto.Url;
            site.Name = dto.Name;
            return Ok(site);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var site = Sites.FirstOrDefault(s => s.Id == id);
            if (site is null) return NotFound();
            Sites.Remove(site);
            return NoContent();
        }
    }

    public record Site
    {
        public int Id { get; set; }
        public string Url { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    public record SiteDto
    {
        public string Url { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
} 
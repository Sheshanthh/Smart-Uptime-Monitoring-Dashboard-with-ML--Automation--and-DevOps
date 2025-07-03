using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartUptime.Api.Models;

namespace SmartUptime.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SitesController : ControllerBase
    {
        private readonly SmartUptimeDbContext _db;
        public SitesController(SmartUptimeDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Site>>> GetAll()
        {
            var sites = await _db.Sites.ToListAsync();
            return Ok(sites);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Site>> Get(int id)
        {
            var site = await _db.Sites.FindAsync(id);
            return site is not null ? Ok(site) : NotFound();
        }

        [HttpPost]
        public async Task<ActionResult<Site>> Create([FromBody] SiteDto dto)
        {
            var site = new Site { Url = dto.Url, Name = dto.Name };
            _db.Sites.Add(site);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = site.Id }, site);
        }

        [HttpPut("{id}")]
        public ActionResult<Site> Update(int id, [FromBody] SiteDto dto)
        {
            var site = _db.Sites.FirstOrDefault(s => s.Id == id);
            if (site is null) return NotFound();
            site.Url = dto.Url;
            site.Name = dto.Name;
            return Ok(site);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var site = _db.Sites.FirstOrDefault(s => s.Id == id);
            if (site is null) return NotFound();
            _db.Sites.Remove(site);
            return NoContent();
        }
    }

    public class SiteDto
    {
        public string Url { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
} 
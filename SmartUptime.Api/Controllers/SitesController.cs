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
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                Console.WriteLine($"Delete request received for site ID: {id}");
                
                var site = await _db.Sites.FindAsync(id);
                if (site is null)
                {
                    Console.WriteLine($"Site with ID {id} not found");
                    return NotFound();
                }
                
                Console.WriteLine($"Found site: {site.Name} ({site.Url})");
                
                // Also delete associated ping results
                var pingResults = await _db.PingResults.Where(p => p.SiteId == id).ToListAsync();
                Console.WriteLine($"Deleting {pingResults.Count} ping results for site {id}");
                _db.PingResults.RemoveRange(pingResults);
                
                _db.Sites.Remove(site);
                
                Console.WriteLine("About to save changes to database...");
                var result = await _db.SaveChangesAsync();
                Console.WriteLine($"SaveChangesAsync returned: {result} rows affected");
                
                // Verify deletion
                var verifySite = await _db.Sites.FindAsync(id);
                if (verifySite == null)
                {
                    Console.WriteLine($"✅ Site {id} successfully deleted from database");
                }
                else
                {
                    Console.WriteLine($"❌ Site {id} still exists in database after deletion!");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting site {id}: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("test-delete/{id}")]
        public async Task<IActionResult> TestDelete(int id)
        {
            var site = await _db.Sites.FindAsync(id);
            if (site is null)
            {
                return NotFound(new { message = $"Site with ID {id} not found" });
            }
            
            return Ok(new { 
                message = $"Site found: {site.Name} ({site.Url})", 
                id = site.Id,
                canDelete = true 
            });
        }

        [HttpGet("debug/all")]
        public async Task<IActionResult> DebugAllSites()
        {
            var sites = await _db.Sites.ToListAsync();
            var pingResults = await _db.PingResults.ToListAsync();
            
            return Ok(new { 
                sites = sites.Select(s => new { s.Id, s.Name, s.Url, s.IsActive }),
                pingResultsCount = pingResults.Count,
                totalSites = sites.Count
            });
        }

        [HttpGet("test-ping/{url}")]
        public async Task<IActionResult> TestPing(string url)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                
                var watch = System.Diagnostics.Stopwatch.StartNew();
                var response = await httpClient.GetAsync(url);
                watch.Stop();
                
                return Ok(new {
                    url = url,
                    statusCode = (int)response.StatusCode,
                    latencyMs = watch.ElapsedMilliseconds,
                    isSuccess = response.IsSuccessStatusCode,
                    headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToArray())
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class SiteDto
    {
        public string Url { get; set; } = string.Empty;
        public string? Name { get; set; }
    }
} 
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SmartUptime.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatusController : ControllerBase
    {
        private static readonly Dictionary<int, SiteStatus> SiteStatuses = new();
        private static readonly Dictionary<int, List<SiteStatusHistory>> SiteStatusHistories = new();

        [HttpGet]
        public ActionResult<IEnumerable<SiteStatus>> GetAllStatuses() => Ok(SiteStatuses.Values);

        [HttpGet("{siteId}")]
        public ActionResult<SiteStatus> GetStatus(int siteId)
        {
            if (!SiteStatuses.TryGetValue(siteId, out var status))
                return NotFound();
            return Ok(status);
        }

        [HttpGet("{siteId}/history")]
        public ActionResult<IEnumerable<SiteStatusHistory>> GetHistory(int siteId)
        {
            if (!SiteStatusHistories.TryGetValue(siteId, out var history))
                return Ok(new List<SiteStatusHistory>());
            return Ok(history);
        }

        public class PingResultDto
        {
            public int Id { get; set; }
            public int SiteId { get; set; }
            public DateTime Timestamp { get; set; }
            public int? LatencyMs { get; set; }
            public int StatusCode { get; set; }
            public bool IsAnomaly { get; set; }
            public string? ErrorMessage { get; set; }
        }

        [HttpGet("/pingresults/recent")]
        public async Task<ActionResult<IEnumerable<PingResultDto>>> GetRecentPingResults([FromServices] SmartUptimeDbContext db, int count = 100)
        {
            var results = await db.PingResults
                .OrderByDescending(p => p.Timestamp)
                .Take(count)
                .Select(p => new PingResultDto
                {
                    Id = p.Id,
                    SiteId = p.SiteId,
                    Timestamp = p.Timestamp,
                    LatencyMs = p.LatencyMs,
                    StatusCode = p.StatusCode,
                    IsAnomaly = p.IsAnomaly,
                    ErrorMessage = p.ErrorMessage
                })
                .ToListAsync();
            return Ok(results);
        }
    }

    public record SiteStatus
    {
        public int SiteId { get; set; }
        public string Status { get; set; } = "Unknown";
        public DateTime CheckedAt { get; set; }
    }

    public record SiteStatusHistory
    {
        public DateTime CheckedAt { get; set; }
        public string Status { get; set; } = "Unknown";
        public bool IsAnomaly { get; set; }
    }
} 
using Microsoft.AspNetCore.Mvc;

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
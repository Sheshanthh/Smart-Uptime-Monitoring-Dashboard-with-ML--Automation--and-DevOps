using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartUptime.Api.Models
{
    public class PingResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("Site")]
        public int SiteId { get; set; }
        public Site Site { get; set; } = null!;

        public DateTime Timestamp { get; set; }

        public int? LatencyMs { get; set; }

        public int StatusCode { get; set; }

        public bool IsAnomaly { get; set; }

        public string? ErrorMessage { get; set; }
    }
} 
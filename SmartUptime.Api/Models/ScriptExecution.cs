using System.ComponentModel.DataAnnotations;

namespace SmartUptime.Api.Models
{
    public class ScriptExecution
    {
        public int Id { get; set; }
        
        [Required]
        public string ScriptName { get; set; } = string.Empty;
        
        [Required]
        public string ScriptPath { get; set; } = string.Empty;
        
        public string? Arguments { get; set; }
        
        [Required]
        public DateTime ExecutedAt { get; set; }
        
        [Required]
        public string TriggerType { get; set; } = string.Empty; // "anomaly", "downtime", "manual"
        
        public int? SiteId { get; set; }
        public Site? Site { get; set; }
        
        public int? PingResultId { get; set; }
        public PingResult? PingResult { get; set; }
        
        [Required]
        public string Status { get; set; } = string.Empty; // "success", "failed", "running"
        
        public int ExitCode { get; set; }
        
        public string? Output { get; set; }
        
        public string? ErrorOutput { get; set; }
        
        public TimeSpan ExecutionTime { get; set; }
        
        public string? ErrorMessage { get; set; }
    }
} 
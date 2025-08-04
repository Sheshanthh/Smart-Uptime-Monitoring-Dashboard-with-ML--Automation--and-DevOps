using System.ComponentModel.DataAnnotations;

namespace SmartUptime.Api.Models
{
    public class EmergencyScript
    {
        public int Id { get; set; }
        
        [Required]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public string ScriptType { get; set; } = string.Empty; // "bash", "powershell", "python"
        
        [Required]
        public string ScriptPath { get; set; } = string.Empty;
        
        public string? DefaultArguments { get; set; }
        
        [Required]
        public string TriggerCondition { get; set; } = string.Empty; // "anomaly", "downtime", "both"
        
        public int AnomalyThreshold { get; set; } = 1; // Number of consecutive anomalies before triggering
        
        public int DowntimeThreshold { get; set; } = 1; // Number of consecutive downtimes before triggering
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastExecuted { get; set; }
        
        public int ExecutionCount { get; set; } = 0;
        
        public int SuccessCount { get; set; } = 0;
        
        public int FailureCount { get; set; } = 0;
        
        public TimeSpan? AverageExecutionTime { get; set; }
        
        public string? Notes { get; set; }
    }
} 
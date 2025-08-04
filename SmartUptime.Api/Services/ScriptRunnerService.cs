using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using SmartUptime.Api.Models;

namespace SmartUptime.Api.Services
{
    public class ScriptRunnerService
    {
        private readonly SmartUptimeDbContext _db;
        private readonly ILogger<ScriptRunnerService> _logger;
        private readonly string _scriptsDirectory;

        public ScriptRunnerService(SmartUptimeDbContext db, ILogger<ScriptRunnerService> logger, IConfiguration configuration)
        {
            _db = db;
            _logger = logger;
            _scriptsDirectory = configuration["ScriptsDirectory"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
        }

        public async Task<bool> ExecuteEmergencyScriptAsync(EmergencyScript script, string triggerType, int? siteId = null, int? pingResultId = null, string? arguments = null, CancellationToken cancellationToken = default)
        {
            var execution = new ScriptExecution
            {
                ScriptName = script.Name,
                ScriptPath = script.ScriptPath,
                Arguments = arguments ?? script.DefaultArguments,
                ExecutedAt = DateTime.UtcNow,
                TriggerType = triggerType,
                SiteId = siteId,
                PingResultId = pingResultId,
                Status = "running"
            };

            _db.ScriptExecutions.Add(execution);
            await _db.SaveChangesAsync();

            var stopwatch = Stopwatch.StartNew();
            bool success = false;
            string output = string.Empty;
            string errorOutput = string.Empty;
            int exitCode = -1;

            try
            {
                _logger.LogInformation("Executing emergency script: {ScriptName} for trigger: {TriggerType}", script.Name, triggerType);

                var processStartInfo = CreateProcessStartInfo(script, arguments);
                using var process = new Process { StartInfo = processStartInfo };

                process.Start();
                
                // Capture output asynchronously
                var outputBuilder = new StringBuilder();
                var errorBuilder = new StringBuilder();
                
                process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        outputBuilder.AppendLine(e.Data);
                        _logger.LogInformation("Script output: {Output}", e.Data);
                    }
                };
                
                process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        errorBuilder.AppendLine(e.Data);
                        _logger.LogWarning("Script error: {Error}", e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // Wait for completion with timeout
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromMinutes(5));
                
                try
                {
                    await process.WaitForExitAsync(cts.Token);
                    exitCode = process.ExitCode;
                    success = exitCode == 0;
                    output = outputBuilder.ToString();
                    errorOutput = errorBuilder.ToString();
                    
                    _logger.LogInformation("Script execution completed with exit code: {ExitCode}", exitCode);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogError("Script execution timed out");
                    process.Kill();
                    success = false;
                    errorOutput = "Script execution timed out after 5 minutes";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing emergency script: {ScriptName}", script.Name);
                errorOutput = ex.ToString();
                success = false;
            }
            finally
            {
                stopwatch.Stop();
                
                // Update execution record
                execution.Status = success ? "success" : "failed";
                execution.ExitCode = exitCode;
                execution.Output = output;
                execution.ErrorOutput = errorOutput;
                execution.ExecutionTime = stopwatch.Elapsed;
                execution.ErrorMessage = success ? null : errorOutput;

                // Update script statistics
                script.LastExecuted = DateTime.UtcNow;
                script.ExecutionCount++;
                if (success)
                    script.SuccessCount++;
                else
                    script.FailureCount++;

                // Calculate average execution time
                if (script.AverageExecutionTime.HasValue)
                {
                    var totalExecutions = script.SuccessCount + script.FailureCount;
                    script.AverageExecutionTime = TimeSpan.FromTicks(
                        (script.AverageExecutionTime.Value.Ticks * (totalExecutions - 1) + stopwatch.Elapsed.Ticks) / totalExecutions
                    );
                }
                else
                {
                    script.AverageExecutionTime = stopwatch.Elapsed;
                }

                await _db.SaveChangesAsync();
            }

            return success;
        }

        private ProcessStartInfo CreateProcessStartInfo(EmergencyScript script, string? arguments)
        {
            var startInfo = new ProcessStartInfo();
            
            switch (script.ScriptType.ToLower())
            {
                case "bash":
                    startInfo.FileName = "bash";
                    startInfo.Arguments = $"{script.ScriptPath} {arguments}";
                    break;
                    
                case "powershell":
                    startInfo.FileName = "powershell.exe";
                    startInfo.Arguments = $"-ExecutionPolicy Bypass -File \"{script.ScriptPath}\" {arguments}";
                    break;
                    
                case "python":
                    startInfo.FileName = "python";
                    startInfo.Arguments = $"{script.ScriptPath} {arguments}";
                    break;
                    
                default:
                    throw new ArgumentException($"Unsupported script type: {script.ScriptType}");
            }

            startInfo.WorkingDirectory = _scriptsDirectory;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            startInfo.CreateNoWindow = true;

            return startInfo;
        }

        public async Task<List<EmergencyScript>> GetActiveScriptsAsync()
        {
            return await _db.EmergencyScripts
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<List<ScriptExecution>> GetRecentExecutionsAsync(int count = 50)
        {
            return await _db.ScriptExecutions
                .Include(se => se.Site)
                .Include(se => se.PingResult)
                .OrderByDescending(se => se.ExecutedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<bool> ShouldTriggerScriptAsync(EmergencyScript script, string triggerType, int siteId)
        {
            if (!script.IsActive) return false;

            // Check if script should be triggered based on condition
            if (script.TriggerCondition == "anomaly" && triggerType != "anomaly") return false;
            if (script.TriggerCondition == "downtime" && triggerType != "downtime") return false;

            // Get recent ping results for this site
            var recentPings = await _db.PingResults
                .Where(p => p.SiteId == siteId)
                .OrderByDescending(p => p.Timestamp)
                .Take(Math.Max(script.AnomalyThreshold, script.DowntimeThreshold))
                .ToListAsync();

            if (!recentPings.Any()) return false;

            // Check for consecutive anomalies
            if (script.TriggerCondition == "anomaly" || script.TriggerCondition == "both")
            {
                var consecutiveAnomalies = recentPings.Take(script.AnomalyThreshold).Count(p => p.IsAnomaly);
                if (consecutiveAnomalies >= script.AnomalyThreshold) return true;
            }

            // Check for consecutive downtimes
            if (script.TriggerCondition == "downtime" || script.TriggerCondition == "both")
            {
                var consecutiveDowntimes = recentPings.Take(script.DowntimeThreshold).Count(p => p.StatusCode < 200 || p.StatusCode >= 300);
                if (consecutiveDowntimes >= script.DowntimeThreshold) return true;
            }

            return false;
        }

        public async Task InitializeDefaultScriptsAsync()
        {
            if (await _db.EmergencyScripts.AnyAsync()) return;

            var defaultScripts = new List<EmergencyScript>
            {
                new EmergencyScript
                {
                    Name = "Restart Service (Bash)",
                    Description = "Restarts a system service using bash script",
                    ScriptType = "bash",
                    ScriptPath = "restart_service.sh",
                    DefaultArguments = "nginx",
                    TriggerCondition = "downtime",
                    DowntimeThreshold = 2,
                    IsActive = true
                },
                new EmergencyScript
                {
                    Name = "Restart Service (PowerShell)",
                    Description = "Restarts a Windows service using PowerShell",
                    ScriptType = "powershell",
                    ScriptPath = "restart_service.ps1",
                    DefaultArguments = "Themes",
                    TriggerCondition = "downtime",
                    DowntimeThreshold = 2,
                    IsActive = true
                },
                new EmergencyScript
                {
                    Name = "Test Script (PowerShell)",
                    Description = "Test script for simulating service restart operations",
                    ScriptType = "powershell",
                    ScriptPath = "test_script.ps1",
                    DefaultArguments = "TestService",
                    TriggerCondition = "downtime",
                    DowntimeThreshold = 1,
                    IsActive = true
                },
                new EmergencyScript
                {
                    Name = "Restart Service (Python)",
                    Description = "Restarts a system service using Python script",
                    ScriptType = "python",
                    ScriptPath = "restart_service.py",
                    DefaultArguments = "apache2",
                    TriggerCondition = "downtime",
                    DowntimeThreshold = 2,
                    IsActive = true
                }
            };

            _db.EmergencyScripts.AddRange(defaultScripts);
            await _db.SaveChangesAsync();
        }
    }
} 
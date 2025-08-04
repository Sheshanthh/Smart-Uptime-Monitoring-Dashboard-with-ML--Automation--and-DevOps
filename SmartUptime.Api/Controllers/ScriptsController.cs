using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartUptime.Api.Models;
using SmartUptime.Api.Services;

namespace SmartUptime.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScriptsController : ControllerBase
    {
        private readonly ScriptRunnerService _scriptRunner;
        private readonly SmartUptimeDbContext _db;
        private readonly ILogger<ScriptsController> _logger;

        public ScriptsController(ScriptRunnerService scriptRunner, SmartUptimeDbContext db, ILogger<ScriptsController> logger)
        {
            _scriptRunner = scriptRunner;
            _db = db;
            _logger = logger;
        }

        [HttpGet("emergency")]
        public async Task<IActionResult> GetEmergencyScripts()
        {
            try
            {
                var scripts = await _db.EmergencyScripts
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                return Ok(scripts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting emergency scripts");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("executions")]
        public async Task<IActionResult> GetScriptExecutions([FromQuery] int count = 50)
        {
            try
            {
                var executions = await _scriptRunner.GetRecentExecutionsAsync(count);
                return Ok(executions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting script executions");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("emergency")]
        public async Task<IActionResult> CreateEmergencyScript([FromBody] EmergencyScript script)
        {
            try
            {
                script.CreatedAt = DateTime.UtcNow;
                _db.EmergencyScripts.Add(script);
                await _db.SaveChangesAsync();
                return CreatedAtAction(nameof(GetEmergencyScripts), new { id = script.Id }, script);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating emergency script");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("emergency/{id}")]
        public async Task<IActionResult> UpdateEmergencyScript(int id, [FromBody] EmergencyScript script)
        {
            try
            {
                var existingScript = await _db.EmergencyScripts.FindAsync(id);
                if (existingScript == null)
                    return NotFound();

                existingScript.Name = script.Name;
                existingScript.Description = script.Description;
                existingScript.ScriptType = script.ScriptType;
                existingScript.ScriptPath = script.ScriptPath;
                existingScript.DefaultArguments = script.DefaultArguments;
                existingScript.TriggerCondition = script.TriggerCondition;
                existingScript.AnomalyThreshold = script.AnomalyThreshold;
                existingScript.DowntimeThreshold = script.DowntimeThreshold;
                existingScript.IsActive = script.IsActive;
                existingScript.Notes = script.Notes;

                await _db.SaveChangesAsync();
                return Ok(existingScript);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating emergency script {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("emergency/{id}")]
        public async Task<IActionResult> DeleteEmergencyScript(int id)
        {
            try
            {
                var script = await _db.EmergencyScripts.FindAsync(id);
                if (script == null)
                    return NotFound();

                _db.EmergencyScripts.Remove(script);
                await _db.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting emergency script {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("emergency/{id}/execute")]
        public async Task<IActionResult> ExecuteScript(int id, [FromBody] ExecuteScriptRequest request)
        {
            try
            {
                var script = await _db.EmergencyScripts.FindAsync(id);
                if (script == null)
                    return NotFound();

                if (!script.IsActive)
                    return BadRequest("Script is not active");

                var success = await _scriptRunner.ExecuteEmergencyScriptAsync(
                    script,
                    request.TriggerType,
                    request.SiteId,
                    request.PingResultId,
                    request.Arguments,
                    HttpContext.RequestAborted
                );

                return Ok(new { success, scriptName = script.Name });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing script {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("executions/{id}")]
        public async Task<IActionResult> GetScriptExecution(int id)
        {
            try
            {
                var execution = await _db.ScriptExecutions
                    .Include(se => se.Site)
                    .Include(se => se.PingResult)
                    .FirstOrDefaultAsync(se => se.Id == id);

                if (execution == null)
                    return NotFound();

                return Ok(execution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting script execution {Id}", id);
                return StatusCode(500, "Internal server error");
            }
        }
    }

    public class ExecuteScriptRequest
    {
        public string TriggerType { get; set; } = string.Empty;
        public int? SiteId { get; set; }
        public int? PingResultId { get; set; }
        public string? Arguments { get; set; }
    }
} 
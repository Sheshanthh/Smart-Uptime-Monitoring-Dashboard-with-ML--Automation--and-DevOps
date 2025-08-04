using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmartUptime.Api.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace SmartUptime.Api.Services
{
    public class SitePingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<SitePingBackgroundService> _logger;
        private const int PingIntervalSeconds = 60;

        public SitePingBackgroundService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory, ILogger<SitePingBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<SmartUptimeDbContext>();
                    var scriptRunner = scope.ServiceProvider.GetRequiredService<ScriptRunnerService>();
                    var sites = db.Sites.Where(s => s.IsActive).ToList();
                    var httpClient = _httpClientFactory.CreateClient();

                    foreach (var site in sites)
                    {
                        _logger.LogInformation("Pinging site {SiteId} ({Url}) at {Time}", site.Id, site.Url, DateTime.UtcNow);
                        var pingResult = new PingResult
                        {
                            SiteId = site.Id,
                            Timestamp = DateTime.UtcNow
                        };
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        try
                        {
                            var httpClientTimeout = _httpClientFactory.CreateClient();
                            httpClientTimeout.Timeout = TimeSpan.FromSeconds(10); // 10s timeout
                            
                            // Add User-Agent to avoid bot detection
                            httpClientTimeout.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                            
                            var response = await httpClientTimeout.GetAsync(site.Url, stoppingToken);
                            watch.Stop();
                            pingResult.LatencyMs = (int)watch.ElapsedMilliseconds;
                            pingResult.StatusCode = (int)response.StatusCode;
                            
                            // Log detailed response info for debugging
                            _logger.LogInformation("Site {SiteId} ({Url}) - Status: {StatusCode}, Latency: {Latency}ms, Headers: {Headers}", 
                                site.Id, site.Url, pingResult.StatusCode, pingResult.LatencyMs, 
                                string.Join(", ", response.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));

                            // ML API call for anomaly detection
                            try
                            {
                                var mlClient = _httpClientFactory.CreateClient();
                                var mlApiUrl = "http://localhost:5000/predict"; // Update if needed
                                var payload = new { latency_ms = pingResult.LatencyMs };
                                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                                var mlResponse = await mlClient.PostAsync(mlApiUrl, content, stoppingToken);
                                if (mlResponse.IsSuccessStatusCode)
                                {
                                    var mlJson = await mlResponse.Content.ReadAsStringAsync();
                                    using var doc = JsonDocument.Parse(mlJson);
                                    if (doc.RootElement.TryGetProperty("anomaly", out var anomalyProp))
                                    {
                                        pingResult.IsAnomaly = anomalyProp.GetInt32() == 1;
                                    }
                                    else
                                    {
                                        pingResult.IsAnomaly = false;
                                    }
                                }
                                else
                                {
                                    _logger.LogWarning("ML API returned non-success status: {StatusCode}", mlResponse.StatusCode);
                                    pingResult.IsAnomaly = false;
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to call ML API for anomaly detection");
                                pingResult.IsAnomaly = false;
                            }

                            _logger.LogInformation("Ping success: Site {SiteId} ({Url}) - Status: {StatusCode}, Latency: {Latency}ms, Anomaly: {IsAnomaly}", site.Id, site.Url, pingResult.StatusCode, pingResult.LatencyMs, pingResult.IsAnomaly);
                        }
                        catch (TaskCanceledException ex) when (!stoppingToken.IsCancellationRequested)
                        {
                            watch.Stop();
                            pingResult.LatencyMs = null;
                            pingResult.StatusCode = 0;
                            pingResult.IsAnomaly = true;
                            pingResult.ErrorMessage = "Timeout: " + ex.Message;
                            _logger.LogWarning(ex, "Ping timeout for site {SiteId} ({Url})", site.Id, site.Url);
                        }
                        catch (HttpRequestException ex)
                        {
                            watch.Stop();
                            pingResult.LatencyMs = null;
                            pingResult.StatusCode = 0;
                            pingResult.IsAnomaly = true;
                            pingResult.ErrorMessage = "Unreachable: " + ex.Message;
                            _logger.LogWarning(ex, "Ping failed (unreachable) for site {SiteId} ({Url})", site.Id, site.Url);
                        }
                        catch (Exception ex)
                        {
                            watch.Stop();
                            pingResult.LatencyMs = null;
                            pingResult.StatusCode = 0;
                            pingResult.IsAnomaly = true;
                            pingResult.ErrorMessage = ex.Message;
                            _logger.LogError(ex, "Ping failed for site {SiteId} ({Url})", site.Id, site.Url);
                        }
                        db.PingResults.Add(pingResult);
                        
                        // Trigger emergency scripts if conditions are met
                        await TriggerEmergencyScriptsAsync(scriptRunner, site, pingResult, stoppingToken);
                    }
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Saved ping results for {Count} sites at {Time}", sites.Count, DateTime.UtcNow);

                    // Clean up old ping results (keep last 7 days)
                    var cutoffDate = DateTime.UtcNow.AddDays(-7);
                    var oldResults = db.PingResults.Where(p => p.Timestamp < cutoffDate);
                    var deletedCount = oldResults.Count();
                    if (deletedCount > 0)
                    {
                        db.PingResults.RemoveRange(oldResults);
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Cleaned up {Count} old ping results older than 7 days", deletedCount);
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(PingIntervalSeconds), stoppingToken);
            }
        }

        private async Task TriggerEmergencyScriptsAsync(ScriptRunnerService scriptRunner, Site site, PingResult pingResult, CancellationToken stoppingToken)
        {
            try
            {
                var activeScripts = await scriptRunner.GetActiveScriptsAsync();
                
                foreach (var script in activeScripts)
                {
                    // Determine trigger type
                    string triggerType = "normal";
                    if (pingResult.IsAnomaly)
                    {
                        triggerType = "anomaly";
                    }
                    else if (pingResult.StatusCode < 200 || pingResult.StatusCode >= 300)
                    {
                        triggerType = "downtime";
                    }

                    // Check if script should be triggered
                    if (await scriptRunner.ShouldTriggerScriptAsync(script, triggerType, site.Id))
                    {
                        _logger.LogInformation("Triggering emergency script {ScriptName} for site {SiteId} due to {TriggerType}", 
                            script.Name, site.Id, triggerType);

                        // Execute the script asynchronously to avoid blocking the ping process
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                var success = await scriptRunner.ExecuteEmergencyScriptAsync(
                                    script, 
                                    triggerType, 
                                    site.Id, 
                                    pingResult.Id, 
                                    script.DefaultArguments,
                                    stoppingToken
                                );

                                _logger.LogInformation("Emergency script {ScriptName} execution completed with success: {Success}", 
                                    script.Name, success);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error executing emergency script {ScriptName}", script.Name);
                            }
                        }, stoppingToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in emergency script triggering for site {SiteId}", site.Id);
            }
        }
    }
} 
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmartUptime.Api.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;

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
                            var response = await httpClientTimeout.GetAsync(site.Url, stoppingToken);
                            watch.Stop();
                            pingResult.LatencyMs = (int)watch.ElapsedMilliseconds;
                            pingResult.StatusCode = (int)response.StatusCode;
                            pingResult.IsAnomaly = false;
                            _logger.LogInformation("Ping success: Site {SiteId} ({Url}) - Status: {StatusCode}, Latency: {Latency}ms", site.Id, site.Url, pingResult.StatusCode, pingResult.LatencyMs);
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
                    }
                    await db.SaveChangesAsync(stoppingToken);
                    _logger.LogInformation("Saved ping results for {Count} sites at {Time}", sites.Count, DateTime.UtcNow);
                }
                await Task.Delay(TimeSpan.FromSeconds(PingIntervalSeconds), stoppingToken);
            }
        }
    }
} 
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using SmartUptime.Api.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace SmartUptime.Api.Services
{
    public class SitePingBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpClientFactory _httpClientFactory;
        private const int PingIntervalSeconds = 60;

        public SitePingBackgroundService(IServiceProvider serviceProvider, IHttpClientFactory httpClientFactory)
        {
            _serviceProvider = serviceProvider;
            _httpClientFactory = httpClientFactory;
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
                        var pingResult = new PingResult
                        {
                            SiteId = site.Id,
                            Timestamp = DateTime.UtcNow
                        };
                        var watch = System.Diagnostics.Stopwatch.StartNew();
                        try
                        {
                            var response = await httpClient.GetAsync(site.Url, stoppingToken);
                            watch.Stop();
                            pingResult.LatencyMs = (int)watch.ElapsedMilliseconds;
                            pingResult.StatusCode = (int)response.StatusCode;
                            pingResult.IsAnomaly = false; // Anomaly detection can be added later
                        }
                        catch (Exception ex)
                        {
                            watch.Stop();
                            pingResult.LatencyMs = null;
                            pingResult.StatusCode = 0;
                            pingResult.IsAnomaly = true;
                            pingResult.ErrorMessage = ex.Message;
                        }
                        db.PingResults.Add(pingResult);
                    }
                    await db.SaveChangesAsync(stoppingToken);
                }
                await Task.Delay(TimeSpan.FromSeconds(PingIntervalSeconds), stoppingToken);
            }
        }
    }
} 
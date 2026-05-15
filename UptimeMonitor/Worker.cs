using System.Diagnostics;
using UptimeMonitor.Data;
using UptimeMonitor.Data.Entities;
namespace UptimeMonitor

{
    public class Worker(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<Worker> logger,
        IHttpClientFactory httpClientFactory) : BackgroundService
    {
        private readonly HttpClient HttpClient = httpClientFactory.CreateClient("monitor");

        private readonly string[] _urls =
            configuration.GetSection("MonitorSettings:Urls").Get<string[]>()
            ?? throw new InvalidOperationException("MonitorSettings:Urls is missing from appsettings.json");



        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var parallelOptions = new ParallelOptions
            {
                CancellationToken = stoppingToken,
                MaxDegreeOfParallelism = 5
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                var tasks = _urls.Select(url => CheckUrlAsync(url, stoppingToken));

                await Task.WhenAll(tasks);

                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task CheckUrlAsync(string url, CancellationToken stoppingToken)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            try
            {

                var response = await HttpClient.GetAsync(url, stoppingToken);

                stopwatch.Stop();
                using var scope = scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<MonitorContext>();


                if (response.IsSuccessStatusCode)
                {
                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        logger.LogWarning(
                            "Request took {responsetime} ms. URL {url} is up but server might be experiencing issues. Status code: {statuscode}",
                            stopwatch.ElapsedMilliseconds,
                            url,
                            response.StatusCode);
                    }
                    else
                    {
                        logger.LogInformation(
                            "Request finished in {responsetime} ms. URL {url} is up. Status code: {statusCode}",
                            stopwatch.ElapsedMilliseconds,
                            url,
                            response.StatusCode);
                    }
                }
                else
                {
                    logger.LogWarning(
                        "Request finished in {responsetime} ms. URL {url} is down. Status code: {statusCode}",
                        stopwatch.ElapsedMilliseconds,
                        url,
                        response.StatusCode);
                }
                LogEntry logEntry = new LogEntry
                {
                    Url = url,
                    StatusCode = response.StatusCode,
                    ResponseTime = stopwatch.ElapsedMilliseconds
                };

                await context.LogEntries.AddAsync(logEntry);
                await context.SaveChangesAsync();
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                logger.LogWarning(
                    "Request timed out after {responsetime} ms. URL {url} is down.",
                    stopwatch.ElapsedMilliseconds,
                    url);
                return;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Failed to reach URL {url}, Time it took {responsetime} ms",
                    url,
                    stopwatch.ElapsedMilliseconds);
                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                stopwatch.Stop();
                logger.LogError(ex, "Unexpected error with the URL {url}", url);
                return;
            }

            
            
        }
    }
}
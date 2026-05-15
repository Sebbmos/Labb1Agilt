using System.Diagnostics;
using System.Net;
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
            using var scope = scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MonitorContext>();

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

                var tasks = _urls.Select(url => CheckUrlAsync(url, context, stoppingToken));

                await Task.WhenAll(tasks);

                await context.SaveChangesAsync(stoppingToken);

                await Task.Delay(10000, stoppingToken);
            }
        }

        private async Task CheckUrlAsync(string url, MonitorContext context, CancellationToken stoppingToken)
        {


            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            string responseMessage;
            var logEntry = new LogEntry()
            {
                Url = url
            };

            try
            {
                var response = await HttpClient.GetAsync(url, stoppingToken);
                stopwatch.Stop();

                if (response.IsSuccessStatusCode)
                {
                    if (stopwatch.ElapsedMilliseconds > 1000)
                    {
                        responseMessage = $"Request finished in {stopwatch.ElapsedMilliseconds} ms. URL {url} is up but server might be experiencing issues. Status code: {response.StatusCode}";

                        logger.LogWarning("{ResponseMessage}", responseMessage);
                    }
                    else
                    {
                        responseMessage = $"Request finished in {stopwatch.ElapsedMilliseconds} ms. URL {url} is up. Status code: {response.StatusCode}";

                        logger.LogInformation("{ResponseMessage}", responseMessage);
                    }
                }
                else
                {
                    responseMessage = $"Request finished in {stopwatch.ElapsedMilliseconds} ms. URL {url} is down. Status code: {response.StatusCode}";

                    logger.LogWarning("{ResponseMessage}", responseMessage);
                }

                logEntry.StatusCode = response.StatusCode;
                logEntry.ResponseTime = stopwatch.ElapsedMilliseconds;
                logEntry.ResponseMessage = responseMessage;
                await context.LogEntries.AddAsync(logEntry, stoppingToken);
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                stopwatch.Stop();
                responseMessage = $"Request timed out after {stopwatch.ElapsedMilliseconds} ms. URL {url} is down.";

                logger.LogWarning("{ResponseMessage}", responseMessage);
                logEntry.ResponseMessage = responseMessage;
                await context.LogEntries.AddAsync(logEntry, stoppingToken);
//                return;
            }
            catch (HttpRequestException ex)
            {
                stopwatch.Stop();
                responseMessage = $"Request failed after {stopwatch.ElapsedMilliseconds} ms. URL {url} is down. Status code: {ex.StatusCode} Error: {ex.Message}";

                logger.LogError("{ResponseMessage}", responseMessage);

                logEntry.StatusCode = ex.StatusCode;
                logEntry.ResponseTime = stopwatch.ElapsedMilliseconds;
                logEntry.ResponseMessage = responseMessage;
                await context.LogEntries.AddAsync(logEntry, stoppingToken);
//                return;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                stopwatch.Stop();
                responseMessage = $"Unexpected error after {stopwatch.ElapsedMilliseconds} ms. URL {url} is down. Error: {ex.Message}";

                logger.LogError("{ResponseMessage}", responseMessage);

                logEntry.ResponseTime = stopwatch.ElapsedMilliseconds;
                logEntry.ResponseMessage = responseMessage;
                await context.LogEntries.AddAsync(logEntry, stoppingToken);
//                return;
            }
        }
    }
}
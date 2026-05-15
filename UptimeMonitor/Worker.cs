namespace UptimeMonitor
{
    public class Worker(
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
            try
            {
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var response = await HttpClient.GetAsync(url, stoppingToken);

                stopwatch.Stop();

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
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Failed to reach URL {url}", url);
            }
        }
    }
}
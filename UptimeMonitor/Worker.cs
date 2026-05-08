namespace UptimeMonitor
{
    public class Worker(IConfiguration configuration, ILogger<Worker> logger) : BackgroundService
    {
        private static readonly HttpClient HttpClient = new();
        private readonly string _url = configuration["MonitorSettings:Url"] ?? throw new InvalidOperationException("MonitorSettings:Url is missing from appsettings.json");

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                try
                {
                    var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                    var response = await HttpClient.GetAsync(_url, stoppingToken);

                    stopwatch.Stop();

                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation("Request finished in {responsetime} ms.URL {url} is up. Status code: {statusCode}", stopwatch.ElapsedMilliseconds, _url, response.StatusCode);
                    }
                    else
                    {
                        logger.LogWarning("Request finished in {responsetime} ms. URL {url} is down. Status code: {statusCode}", stopwatch.ElapsedMilliseconds, _url, response.StatusCode);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Failed to reach URL {url}", _url);
                }

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}

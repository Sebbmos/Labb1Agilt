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
                    var response = await HttpClient.GetAsync(_url, stoppingToken);
                    if (response.IsSuccessStatusCode)
                    {
                        logger.LogInformation("URL {url} is up. Status code: {statusCode}", _url, response.StatusCode);
                    }
                    else
                    {
                        logger.LogWarning("URL {url} is down. Status code: {statusCode}", _url, response.StatusCode);
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

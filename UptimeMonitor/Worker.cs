namespace UptimeMonitor
{
    public class Worker(string url, ILogger<Worker> logger) : BackgroundService
    {
        private static HttpClient _httpClient = new();
        public string Url { get; set; } = url;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }

                var response = await _httpClient.GetAsync(Url, stoppingToken);
                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("URL {url} is up. Status code: {statusCode}", Url, response.StatusCode);
                }
                else
                {
                    logger.LogWarning("URL {url} is down. Status code: {statusCode}", Url, response.StatusCode);
                }
            }
        }
    }
}

using Polly;
using Polly.Extensions.Http;

namespace UptimeMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services
                .AddOptions<MonitorSettings>()
                .Bind(builder.Configuration.GetSection("MonitorSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            builder.Services.AddHttpClient("monitor")
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromSeconds(2)));

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
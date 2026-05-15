using Polly;
using Polly.Extensions.Http;

using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Data;

namespace UptimeMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);

            builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

            builder.Services
                .AddOptions<MonitorSettings>()
                .Bind(builder.Configuration.GetSection("MonitorSettings"))
                .ValidateDataAnnotations()
                .ValidateOnStart();


            builder.Services.AddHttpClient("monitor", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(5);
            })
                .AddPolicyHandler(HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .Or<TaskCanceledException>()
                    .WaitAndRetryAsync(3, retryAttempt =>
                        TimeSpan.FromMilliseconds(300)));

            var connectionString = builder.Configuration["ConnectionStrings:SQLiteDefault"] ?? throw new InvalidOperationException("Connection string not found");
            builder.Services.AddDbContext<MonitorContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<MonitorContext>();
                    context.Database.Migrate();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Worker>>();
                    logger.LogError(ex, "An error occurred while migrating the database.");
                    throw;
                }
            }
            
            host.Run();
        }
    }
}
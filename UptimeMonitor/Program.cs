using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Data;

namespace UptimeMonitor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

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
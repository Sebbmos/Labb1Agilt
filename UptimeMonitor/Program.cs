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

            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}
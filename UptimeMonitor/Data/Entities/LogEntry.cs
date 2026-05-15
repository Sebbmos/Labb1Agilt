using System.Net;

namespace UptimeMonitor.Data.Entities;

public class LogEntry
{
    public Guid Id { get; set; }
    public string Url { get; set; } = null!;
    public HttpStatusCode? StatusCode { get; set; }
    public long ResponseTime { get; set; }
    public DateTime Date { get; set; }
    public string ResponseMessage { get; set; } = null!;

    public override string ToString()
    {
        return $"{Url}\n{StatusCode}\n{ResponseTime}\n{ResponseMessage}";
    }
}
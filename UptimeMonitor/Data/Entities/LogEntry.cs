using System.Net;

namespace UptimeMonitor.Data.Entities;

public class LogEntry
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public int ResponseTime { get; set; }
    public DateTime Date { get; set; }
}
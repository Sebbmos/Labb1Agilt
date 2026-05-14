using System.ComponentModel.DataAnnotations;

namespace UptimeMonitor
{
    public class MonitorSettings
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one URL is required.")]
        public string[] Urls { get; set; } = [];
    }
}
using Microsoft.EntityFrameworkCore;
using UptimeMonitor.Data.Entities;

namespace UptimeMonitor.Data;

public class MonitorContext : DbContext
{
    public MonitorContext(DbContextOptions<MonitorContext> options) : base(options) { }
    public DbSet<LogEntry> LogEntries => Set<LogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LogEntry>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Url)
                .IsRequired();
            
            entity.Property(e => e.StatusCode)
                .HasConversion<int>()
                .IsRequired();

            entity.Property(e => e.ResponseTime);
            
            entity.Property(e => e.Date)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }
}
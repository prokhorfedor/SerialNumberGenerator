using Microsoft.EntityFrameworkCore;

namespace Database;

public class WorkOrderContext : DbContext
{
    public DbSet<InventoryEntry> InventoryEntries { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }

    public WorkOrderContext(DbContextOptions<WorkOrderContext> options) : base(options) { }

    public Task<string> GetLastSerialNumberAsync()
    {
        return Task.FromResult(string.Empty);
    }

    public Task SaveNewLastSerialNumberAsync(string serialNumber)
    {
        return Task.CompletedTask;
    }
}
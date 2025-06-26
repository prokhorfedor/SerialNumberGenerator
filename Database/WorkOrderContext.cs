using Contracts;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class WorkOrderContext : DbContext
{
    public DbSet<InventoryEntry> InventoryEntries { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }

    public WorkOrderContext(DbContextOptions<WorkOrderContext> options) : base(options) { }

    public async Task<string> GetLastSerialNumberAsync()
    {
        var lastStoredSerialNumber = await this.Database.SqlQuery<string>($"SELECT TOP 1 [LSTSERN] FROM [SERNUM]")
            .FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(lastStoredSerialNumber))
        {
            lastStoredSerialNumber = await this.InventoryEntries
                .Where(i => i.IdKey == GeneratorConstants.SERIAL_NUMBER_ID_KEY)
                .OrderByDescending(i => i.SavedDateTime).ThenByDescending(i => i.InventoryNumber)
                .Select(i => i.InventoryNumber).FirstOrDefaultAsync();
        }

        return lastStoredSerialNumber ?? string.Empty;
    }

    public Task SaveNewLastSerialNumberAsync(string serialNumber)
    {
        return Task.CompletedTask;
    }
}
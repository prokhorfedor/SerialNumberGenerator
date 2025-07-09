using Contracts;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class WorkOrderContext : DbContext
{
    public DbSet<InventoryEntry> InventoryEntries { get; set; }
    public DbSet<WorkOrder> WorkOrders { get; set; }
    public DbSet<WorkOrderSerialized> WorkOrdersSerialized { get; set; }

    public WorkOrderContext(DbContextOptions<WorkOrderContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkOrder>().HasOne<WorkOrderSerialized>(wo => wo.WorkOrderSerialized).WithOne()
            .HasForeignKey<WorkOrderSerialized>(wo => wo.WorkOrderId);
        base.OnModelCreating(modelBuilder);
    }

    public async Task<string> GetLastSerialNumberAsync()
    {
        await using var transaction = await this.Database.BeginTransactionAsync();
        try
        {
            var lastStoredSerialNumber = await this.Database.SqlQuery<string>($"SELECT TOP 1 [LSTSERN] as value FROM [SERNUM]")
                .FirstOrDefaultAsync();
            if (string.IsNullOrWhiteSpace(lastStoredSerialNumber))
            {
                lastStoredSerialNumber = await this.InventoryEntries
                    .Where(i => i.IdKey == GeneratorConstants.SERIAL_NUMBER_ID_KEY)
                    .OrderByDescending(i => i.SavedDateTime).ThenByDescending(i => i.SerialNumber)
                    .Select(i => i.SerialNumber).FirstOrDefaultAsync();
            }

            await transaction.CommitAsync();
            return lastStoredSerialNumber ?? string.Empty;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task SaveNewLastSerialNumberAsync(string serialNumber)
    {
        await using var transaction = await this.Database.BeginTransactionAsync();
        try
        {
            await this.Database.ExecuteSqlAsync($"TRUNCATE TABLE [SERNUM]");
            await this.Database.ExecuteSqlAsync($"INSERT INTO [SERNUM] ([LSTSERN]) VALUES ({serialNumber})");
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(e);
            throw;
        }

        return;
    }
}
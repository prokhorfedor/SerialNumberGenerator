using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

[Table("INVTSER")]
public class InventoryEntry
{
    [Key] [Column("SERIALUNIQ")] public string InventoryNumber { get; set; }
    [Column("SERIALNO")] public string SerialNumber { get; set; }
    [Column("WONO")] public string WorkOrderId { get; set; }
    [Column("ID_KEY")] public string IdKey { get; set; }
    [Column("SAVEDTTM")] public DateTime SavedDateTime { get; set; }
}
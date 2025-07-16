using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

[Table("WOENTRY")]
public class WorkOrder
{
    [Key]
    [Column("WONO", TypeName = "nvarchar(10)")]
    public string WorkOrderId { get; set; }

    [Column("SERIALYES", TypeName = "bit")]
    public bool HasSerialNumber { get; set; }

    [Column("OPENCLOS", TypeName = "nvarchar(10)")]
    public string OpenClose { get; set; }

    [Column("JobType", TypeName = "nvarchar(30)")]
    public string? JobType { get; set; }

    [Column("BLDQTY")] public decimal BuildQuantity { get; set; }

    public WorkOrderSerialized? WorkOrderSerialized { get; set; }
}
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Database;

[Table("udfWork_Order")]
public class WorkOrderSerialized
{
    [Key]
    [Column("udfId")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Column("fkWONO", TypeName = "nvarchar(10)")]
    public string WorkOrderId { get; set; }

    [Column("Is_Serial_Number_Generated", TypeName = "bit")]
    public bool? IsSerialNumberGenerated { get; set; }
}
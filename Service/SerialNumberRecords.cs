using CsvHelper.Configuration;

namespace Service;

public class SerialNumberRecords
{
    public string WorkOrderId { get; set; }
    public string SerialNumber { get; set; }
}

public class SerialNumberRecordClassMap : ClassMap<SerialNumberRecords>
{
    public SerialNumberRecordClassMap()
    {
        Map(m => m.WorkOrderId).Index(0).Name("WONO");
        Map(m => m.SerialNumber).Index(1).Name("SerialNO");
    }
}
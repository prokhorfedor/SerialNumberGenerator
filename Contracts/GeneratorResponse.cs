namespace Contracts;

public class GeneratorResponse
{
    /// <summary>
    /// Work orders processed
    /// </summary>
    public int WorkOrdersCount { get; set; }
    /// <summary>
    /// Total Serial Numbers generated
    /// </summary>
    public decimal SerialNumbersGeneratedCount { get; set; }
    /// <summary>
    /// Last generated Serial Number
    /// </summary>
    public string LastGeneratedSerialNumber { get; set; } = string.Empty;
    /// <summary>
    /// File path with newly generated csv-file
    /// </summary>
    public string FilePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
}
namespace Contracts;

public class GeneratorResponse
{
    public int WorkOrdersCount { get; set; }
    public int SerialNumbersGeneratedCount { get; set; }
    public string LastGeneratedSerialNumber { get; set; } = string.Empty;
    public string FilePath { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
}
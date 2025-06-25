namespace Contracts;

public class LastSerialNumber(string serialNumber)
{
    public string SerialNumber { get; private set; } = serialNumber;

    private int _numberPart = -1;
    public int NumberPart => GetNumberPart();

    public void SetNewSerialNumber(string serialNumber)
    {
        SerialNumber = serialNumber;
        _numberPart = -1;
    }

    private int GetNumberPart()
    {
        if (_numberPart != -1)
            return _numberPart;

        var endingIndex = SerialNumber.IndexOf(GeneratorConstants.SERIAL_NUMBER_ENDING,
            StringComparison.OrdinalIgnoreCase);
        var numberPart = endingIndex < 0 ? string.Empty : SerialNumber.Remove(endingIndex);
        return int.TryParse(numberPart, out var result) ? result : 0;
    }

}
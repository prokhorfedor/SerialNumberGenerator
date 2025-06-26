namespace Contracts;

public class LastSerialNumber
{
    // Full string Serial Number 000000000000000000000000000-DA
    public string SerialNumber { get; private set; }

    private int _numberPart = -1;
    // Only number part of the Serial Number, example: 123
    public int NumberPart => GetNumberPart();

    public LastSerialNumber(string serialNumber)
    {
        SerialNumber = string.IsNullOrWhiteSpace(serialNumber)
            ? GeneratorConstants.SERIAL_NUMBER_ENDING.PadLeft(GeneratorConstants.SERIAL_NUMBER_LENGTH, '0')
            : serialNumber;
    }

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
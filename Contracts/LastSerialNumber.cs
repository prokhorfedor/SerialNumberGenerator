namespace Contracts;

public class LastSerialNumber
{
    // Full string Serial Number 000000000000000000000000123
    public string SerialNumber { get; private set; }

    private int _numberPart = -1;
    // Only number part of the Serial Number, example: 123
    public int NumberPart => GetNumberPart();

    public LastSerialNumber(string serialNumber)
    {
        SerialNumber = string.IsNullOrWhiteSpace(serialNumber)
            ? string.Empty.PadLeft(GeneratorConstants.SERIAL_NUMBER_LENGTH, '0')
            : serialNumber.PadLeft(GeneratorConstants.SERIAL_NUMBER_LENGTH, '0');
    }

    public void SetNewSerialNumber(string serialNumber)
    {
        SerialNumber = serialNumber.PadLeft(GeneratorConstants.SERIAL_NUMBER_LENGTH, '0');
        _numberPart = -1;
    }

    private int GetNumberPart()
    {
        if (_numberPart != -1)
            return _numberPart;

        var endingIndex = SerialNumber.IndexOf(GeneratorConstants.SERIAL_NUMBER_ENDING,
            StringComparison.OrdinalIgnoreCase);
        var numberPart = endingIndex < 0 ? SerialNumber : SerialNumber.Remove(endingIndex);
        var numberPartNum = int.TryParse(numberPart, out var result) ? result : 0;
        _numberPart = numberPartNum == 0 ? 0 : numberPartNum;
        return numberPartNum;
    }

}
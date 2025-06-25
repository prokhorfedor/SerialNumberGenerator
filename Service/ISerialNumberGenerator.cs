using Contracts;

namespace Service;

public interface ISerialNumberGenerator
{
    Task<GeneratorResponse> GenerateSerialNumbersFileAsync(string? userFilePath);
}
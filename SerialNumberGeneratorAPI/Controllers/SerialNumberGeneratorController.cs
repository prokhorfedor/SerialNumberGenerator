using Contracts;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace SerialNumberGeneratorAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class SerialNumberGeneratorController : ControllerBase
{
    private readonly ISerialNumberGenerator _serialNumberGenerator;
    private readonly ILogger<SerialNumberGeneratorController> _logger;

    public SerialNumberGeneratorController(ISerialNumberGenerator serialNumberGenerator, ILogger<SerialNumberGeneratorController> logger)
    {
        _serialNumberGenerator = serialNumberGenerator;
        _logger = logger;
    }

    [HttpGet(Name = "GenerateSerialNumbers")]
    public async Task<ActionResult<GeneratorResponse>> Get()
    {
        try
        {
            var generatorResponse = await _serialNumberGenerator.GenerateSerialNumbersFileAsync();
            return Ok(generatorResponse);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}


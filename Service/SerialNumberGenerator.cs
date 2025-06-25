using System.Globalization;
using Contracts;
using CsvHelper;
using CsvHelper.Configuration;
using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Service;

public class SerialNumberGenerator : ISerialNumberGenerator
{
    private readonly WorkOrderContext _woContext;
    private readonly IConfiguration _configuration;

    public SerialNumberGenerator(WorkOrderContext woContext, IConfiguration configuration)
    {
        _woContext = woContext;
        _configuration = configuration;
    }

    public async Task<GeneratorResponse> GenerateSerialNumbersFileAsync(string? userFilePath)
    {
        try
        {
            var generatorResponse = new GeneratorResponse();
            var configFilePath = _configuration.GetSection("AppSettings")["FilePath"];
            generatorResponse.FilePath = !string.IsNullOrWhiteSpace(userFilePath)
                ? userFilePath
                : !string.IsNullOrWhiteSpace(configFilePath)
                    ? configFilePath
                    : generatorResponse.FilePath;

            var lastSavedSerialNumber = await _woContext.GetLastSerialNumberAsync();
            var lastSerialNumberObj = new LastSerialNumber(lastSavedSerialNumber);

            var newOrders = await (from order in _woContext.WorkOrders
                where order.HasSerialNumber && order.OpenClose == GeneratorConstants.WORKORDER_OPEN_STATUS &&
                      !_woContext.InventoryEntries.Any(i => i.WorkOrderId == order.WorkOrderId)
                select order).ToListAsync();


            generatorResponse.WorkOrdersCount = newOrders.Count;
            generatorResponse.SerialNumbersGeneratedCount = newOrders.Sum(o => o.BuildQuantity);
            var currentNumberPart = lastSerialNumberObj.NumberPart;

            var records = new List<SerialNumberRecords>();
            foreach (var order in newOrders)
            {
                for (int i = 0; i < order.BuildQuantity; i++)
                {
                    records.Add(new SerialNumberRecords()
                    {
                        WorkOrderId = order.WorkOrderId,
                        SerialNumber = $"{++currentNumberPart}{GeneratorConstants.SERIAL_NUMBER_ENDING}",
                    });
                }
            }

            await using (var writer =
                         new StreamWriter($"{generatorResponse.FilePath}\\wo_ser_{DateTime.Now:yyyyMMdd}.csv"))
            await using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<SerialNumberRecordClassMap>();
                await csv.WriteRecordsAsync(records);
            }

            var newLastSerialNumber =
                $"{(lastSerialNumberObj.NumberPart + generatorResponse.SerialNumbersGeneratedCount)}{GeneratorConstants.SERIAL_NUMBER_ENDING}";
            lastSerialNumberObj.SetNewSerialNumber(newLastSerialNumber);
            await _woContext.SaveNewLastSerialNumberAsync(lastSerialNumberObj.SerialNumber);
            generatorResponse.LastGeneratedSerialNumber = lastSerialNumberObj.SerialNumber;
            return generatorResponse;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
using System.Globalization;
using Contracts;
using CsvHelper;
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

            var lastSavedSerialNumber = await _woContext.GetLastSerialNumberAsync();
            var lastSerialNumberObj = new LastSerialNumber(lastSavedSerialNumber);

            var newOrders = await (from order in _woContext.WorkOrders.Include(wo => wo.WorkOrderSerialized)
                where order.HasSerialNumber && order.OpenClose == GeneratorConstants.WORKORDER_OPEN_STATUS &&
                      !_woContext.InventoryEntries.Any(i => i.WorkOrderId == order.WorkOrderId)
                      && (order.WorkOrderSerialized == null || !order.WorkOrderSerialized.IsSerialNumberGenerated)
                select order).ToListAsync();

            if (newOrders.Count == 0)
            {
                generatorResponse.LastGeneratedSerialNumber = lastSerialNumberObj.SerialNumber;
                return generatorResponse;
            }

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

                if (order.WorkOrderSerialized != null)
                {
                    order.WorkOrderSerialized.IsSerialNumberGenerated = true;
                }
                else
                {
                    await _woContext.WorkOrdersSerialized.AddAsync(new WorkOrderSerialized()
                    {
                        WorkOrderId = order.WorkOrderId,
                        IsSerialNumberGenerated = true
                    });
                }
            }

            await _woContext.SaveChangesAsync();

            var configFilePath = _configuration.GetSection("AppSettings")["FilePath"];
            generatorResponse.FilePath = !string.IsNullOrWhiteSpace(userFilePath)
                ? userFilePath
                : !string.IsNullOrWhiteSpace(configFilePath)
                    ? configFilePath
                    : generatorResponse.FilePath;
            await using (var writer =
                         new StreamWriter(Path.Combine(generatorResponse.FilePath, $"wo_ser_{DateTime.Now:yyyyMMddHHmmss}.csv")))
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
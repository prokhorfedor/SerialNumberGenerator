using Contracts;
using Database;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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

            var filePath = Path.Combine(generatorResponse.FilePath, $"wo_ser_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
            {
                // get sheet data
                SheetData mySheetData = MakeSheetData(records);

                // Add a WorkbookPart to the document.
                WorkbookPart workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                // Add a WorksheetPart to the WorkbookPart.
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet(mySheetData);

                // Add Sheets to the Workbook.
                Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());

                // Append a new worksheet and associate it with the workbook.
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "serialNumbers" };
                sheets.Append(sheet);
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

    private SheetData MakeSheetData(List<SerialNumberRecords> recordsList)
    {
        var workOrderColumnIndex = 1;
        var serialNumberColumnIndex = 2;
        //set the row count for header
        int rowCount = 1;

        //create the header
        Row header = new Row();

        //create the row index
        header.RowIndex = (uint)rowCount;

        //the sheet itself to return
        SheetData sheetData = new SheetData();

        // Add WorkOrder column
        Cell workOrderHeaderCell = CreateCell(workOrderColumnIndex, rowCount, "WONO");

        // append the cell to the row (header row in this case)
        header.AppendChild(workOrderHeaderCell);

        // Add SerialNumber column
        Cell serialNumberHeaderCell = CreateCell(serialNumberColumnIndex, rowCount, "SerialNO");

        // append the cell to the row (header row in this case)
        header.AppendChild(serialNumberHeaderCell);

        //and now another row...
        rowCount++;

        //dont forget to append header!
        sheetData.AppendChild(header);

        //for each element in my list, every is pretty the same
        foreach (var record in recordsList)
        {
            Row newRow = new Row();
            newRow.RowIndex = (uint)rowCount;
            Cell workOrderCell = CreateCell(workOrderColumnIndex, rowCount, record.WorkOrderId);
            newRow.AppendChild(workOrderCell);
            Cell serialNumberCell = CreateCell(serialNumberColumnIndex, rowCount, record.SerialNumber);
            newRow.AppendChild(serialNumberCell);
            sheetData.AppendChild(newRow);
            rowCount++;
        }

        return sheetData;
    }

    private Cell CreateCell(int positionX, int positionY, string data)
    {
        //this only will be for the reference of the cell, coming from my row count and my column count
        int unicode = 64 + positionX;
        char character = (char)unicode;
        Cell newCell = new Cell() { CellReference = character.ToString() + positionY };

        //the data itself
        newCell.CellValue = new CellValue(data);
        newCell.DataType = new EnumValue<CellValues>(CellValues.String);
        return newCell;
    }
}
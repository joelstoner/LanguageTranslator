
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

class FiniteStateTable
{
    private static string GetCellValue(Cell theCell, WorkbookPart wbPart)
    {
        string? value;
        
        if (theCell is null || theCell.InnerText.Length < 0)
        {
            return string.Empty;
        }
        value = theCell.InnerText;
        if (theCell.DataType is not null)
        {
            if (theCell.DataType.Value == CellValues.SharedString)
            {
                var stringTable = wbPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                if (stringTable is not null)
                {
                    value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
                }
            }
            else if (theCell.DataType.Value == CellValues.Boolean)
            {
                switch (value)
                {
                    case "0":
                        value = "FALSE";
                        break;
                    default:
                        value = "TRUE";
                        break;
                }
            }
        }


        return value;
}

    private static int[] GetSymbolTableDimensions(string filename)
    {
        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
        {
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            
            int numCol = sheetData.Elements<Row>().FirstOrDefault()!.Elements<Cell>().Count();
            int numRow = sheetData.Elements<Row>().Count();
            return [numRow, numCol];
        }
    }

    private static void ScanSymbolTable(string filename, string[,] table)
    {
        using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
        {
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart ?? spreadsheetDocument.AddWorkbookPart();
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
            SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
            
            int rowCount = 0;
            foreach (Row r in sheetData.Elements<Row>())
            {
                int colCount = 0;
                foreach (Cell c in r.Elements<Cell>())
                {
                    table[rowCount,colCount] = GetCellValue(c, workbookPart);
                    colCount++;
                }
                rowCount++;
            }
        }
    }


    internal string[,] GetFiniteStateTable()
    {
        string xlfile = "/home/joelstoner/RiderProjects/TestProject/LexicalAnalyzer/ExcelSymbolTable.xlsx";
        int[] dimensions = GetSymbolTableDimensions(xlfile);
        string[,] symbolTable = new string[dimensions[0], dimensions[1]];
        ScanSymbolTable(xlfile, symbolTable); 
        /* for (int i = 0; i < dimensions[0]; i++)
        {
            for (int j = 0; j < dimensions[1]; j++)
            {
                Console.Write(symbolTable[i, j] + " ");
            }
            Console.WriteLine();
        } */
        return symbolTable;
    }
    
}

using System;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Excel = Microsoft.Office.Interop.Excel;

public class libexcel : IDisposable
{
    private Excel.Application app;
    private Excel.Worksheet ws;
    private Excel.Workbook wb;
    private static object blank = System.Reflection.Missing.Value;
    public bool open(string filename, bool read_only, bool visible)
    {
        System.IO.FileInfo fi = new System.IO.FileInfo(filename);
        if (fi.Exists)
        {
            app = new Excel.Application();
            wb = app.Workbooks.Open(filename, false, read_only, blank, blank, blank, blank, blank, blank, blank, blank, blank, blank);
            ws = (Excel.Worksheet)app.ActiveWorkbook.Sheets[1];
            if (visible) app.Visible = true;
            return true;
        }
        return false;
    }
    public void newexcel()
    {
        app = new Excel.Application();
        wb = app.Workbooks.Add(blank);
        ws = (Excel.Worksheet)app.ActiveWorkbook.Sheets[1];
    }
    public void formatcolumns(double[] width)
    {
        Excel.Range range;
        for (int i = 1; i < width.Length; i++)
        {
            range = (Excel.Range)ws.Cells[i, i];
            range.EntireColumn.ColumnWidth = width[i - 1];
        }
    }
    //public void setsheets(int count)
    //{
    //    while (app.ActiveWorkbook.Sheets.Count < count) app.ActiveWorkbook.Sheets.Add(blank, blank, blank, blank);
    //}
    //public void excelreport(int num, string filename, string report, string name, bool show)
    //{
    //    ws = (Excel.Worksheet)app.ActiveWorkbook.Sheets[num];
    //    Excel.Workbook wb2 = app.Workbooks.Open(filename, false, true, blank, blank, blank, blank, blank, blank, blank, blank, blank, blank);
    //    Excel.Sheets sh2 = app.ActiveWorkbook.Sheets;
    //    Excel.Worksheet ws2 = (Excel.Worksheet)app.ActiveWorkbook.Sheets[1];
    //    ws2.Copy(blank, ws);
    //    app.DisplayAlerts = false;
    //    ws.Delete();
    //    app.DisplayAlerts = true;
    //    ws = (Excel.Worksheet)app.ActiveWorkbook.Sheets[num];
    //    ws.Name = report;
    //    wb2.Close(false, blank, blank);
    //    if (show) app.Visible = true;
    //}
    public void excelreport(int num, DataTable dt, string report, string name, bool show, bool autofit, bool significantDigitsOnly = false)
    {
        wb.Title = name;
        if (num > 1)
        {
            app.ActiveWorkbook.Sheets.Add(blank, blank, 1, blank);
            num = 1;
        }
        ws = (Excel.Worksheet)app.ActiveWorkbook.Sheets[num];
        excelreport(dt, report, show, autofit, significantDigitsOnly: significantDigitsOnly);
    }
    //public void excelreport(int num, DataTable dt, string report, string name, bool show, double[] width, bool grid)
    //{
    //    excelreport(num, dt, report, name, show, width);
    //    if (grid) gridlines();
    //}
    public void excelreport(int num, DataTable dt, string report, string name, bool show, double[] width, bool significantDigitsOnly = false)
    {
        wb.Title = name;
        if (num > 1)
        {
            app.ActiveWorkbook.Sheets.Add(blank, blank, 1, blank);
            num = 1;
        }
        ws = (Excel.Worksheet)app.ActiveWorkbook.Sheets[num];
        formatcolumns(width);
        excelreport(dt, report, show, false, significantDigitsOnly: significantDigitsOnly);
    }
    public void excelreport(DataTable dt, string report, bool show, bool autofit, bool significantDigitsOnly = false, bool includeColumnHeaders = false, int columnHeaderRow = 1, bool landscape = false, BackgroundWorker backgroundWorker = null)
    {
        ws.Name = report;
        string maxcol = dt.Columns[dt.Columns.Count - 1].ColumnName;

        for (int i = 1; i <= dt.Rows.Count; i++)
        {
            for (int j = 1; j <= dt.Columns.Count; j++)
            {
                if (backgroundWorker != null && backgroundWorker.CancellationPending)
                {
                    return;
                }

                string tmp = dt.Rows[i - 1][j - 1].ToString();
                if (tmp.Length > 0)
                {
                    Excel.Range range = (Excel.Range)ws.Cells[i, j];
                    if (tmp.Contains("<b>"))
                    {
                        tmp = tmp.Replace("<b>", "");
                        range.Font.Bold = true;
                    }
                    if (tmp.Contains("<i>"))
                    {
                        tmp = tmp.Replace("<i>", "");
                        range.Font.Italic = true;
                    }
                    if (tmp.Contains("<u>"))
                    {
                        tmp = tmp.Replace("<u>", "");
                        range.Font.Underline = true;
                    }
                    if (tmp.Contains("<c>"))
                    {
                        tmp = tmp.Replace("<c>", "");
                        range.HorizontalAlignment = Excel.Constants.xlCenter;
                    }
                    if (tmp.Contains("<l>"))
                    {
                        tmp = tmp.Replace("<l>", "");
                        range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, 1);
                    }
                    if (tmp.Contains("<red>"))
                    {
                        tmp = tmp.Replace("<red>", "");
                        range.Font.Color = 255;
                    }
                    if (tmp.Contains("<border>"))
                    {
                        tmp = tmp.Replace("<border>", "");
                        range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlThick, Excel.XlColorIndex.xlColorIndexAutomatic, System.Drawing.ColorTranslator.ToOle(Color.Red));
                    }
                    if (tmp.Contains("<y>"))
                    {
                        tmp = tmp.Replace("<y>", "");
                        range = ws.Range["A" + i, "A" + (i + 6)];
                        range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, 1);
                        range = ws.Range["B" + i, "B" + (i + 6)];
                        range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, 1);
                        range = ws.Range["C" + i, "C" + (i + 6)];
                        range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, 1);
                        range = ws.Range["D" + i, "D" + (i + 6)];
                        range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, 1);
                        range = ws.Range["E" + i, "E" + (i + 6)];
                        range.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, 1);
                    }
                    if (tmp.Contains("<picklistrow>"))
                    {
                        tmp = tmp.Replace("<picklistrow>", "");
                        var objPicklistRowRange = ws.Range["A" + i, "G" + i];
                        objPicklistRowRange.BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium, Excel.XlColorIndex.xlColorIndexAutomatic, 1);
                    }
                    if (tmp.Contains("<image>"))
                    {
                        tmp = tmp.Replace("<image>", "");
                        addimage(i, j, tmp);
                    }
                    if (tmp.Contains("<formula>"))
                    {
                        tmp = tmp.Replace("<formula>", "");
                        ws.Cells[i, j] = tmp;
                        ((Excel.Range)ws.Cells[i, j]).Formula = tmp;
                        continue;
                    }
                    if (tmp.Contains("<note>"))
                    {
                        tmp = tmp.Replace("<note>", "");
                        range = ws.Range["A" + i, maxcol + i];
                        range.Merge(0);
                        double lines = 1;
                        int ct = 0;
                        for (int k = 0; k < tmp.Length; k++)
                        {
                            if (tmp.Substring(k, 1) == "\n")
                            {
                                lines++;
                                ct = 0;
                            }
                            else ct++;
                            if (ct > 95)
                            {
                                lines++;
                                ct = 0;
                            }
                            if (lines == 32)
                            {
                                range.EntireRow.RowHeight = (double)range.EntireRow.RowHeight * lines;
                                range.WrapText = true;
                                lines = 0;
                                ws.Cells[i, j] = tmp.Substring(0, k);
                                tmp = tmp.Substring(k);
                                i++;
                                range = ws.Range["A" + i, maxcol + i];
                                range.Merge(0);
                            }
                        }
                        range.EntireRow.RowHeight = (double)range.EntireRow.RowHeight * lines;
                        range.WrapText = true;
                    }

                    // Set cell type (NumberFormat).
                    if (tmp.Contains("<cur>"))
                    {
                        tmp = tmp.Replace("<cur>", "");
                        range.NumberFormat = "0.00";
                    }
                    else if (tmp.Length > 0)
                    {
                        decimal tempDecimal;
                        if (decimal.TryParse(tmp, out tempDecimal))
                        {
                            string[] splitString = tmp.Split('.');
                            string afterDecimal = splitString.Length > 1 ? splitString[1] : "";
                            // If there is a decimal place...
                            if (afterDecimal.Length > 0)
                            {
                                if (significantDigitsOnly)
                                {
                                    // Format as a number to the last significant digit.
                                    var numberFormat = new StringBuilder();
                                    for (int k = 0; k < afterDecimal.Length; k++)
                                    {
                                        if (afterDecimal[k] != '0')
                                        {
                                            numberFormat.Append("0");
                                        }
                                        else
                                        {
                                            int remainingDigitsAsInt;
                                            int.TryParse(afterDecimal.Substring(k), out remainingDigitsAsInt);
                                            if (remainingDigitsAsInt == 0) break;
                                            numberFormat.Append("0");
                                        }
                                    }
                                    if (numberFormat.Length > 0) numberFormat.Insert(0, "0.");
                                    else numberFormat.Append("0");
                                    range.NumberFormat = numberFormat.ToString();
                                }
                                else
                                {
                                    range.NumberFormat = "0.".PadRight(afterDecimal.Length + 2, '0'); // Format as a number to the last digit.
                                }
                            }
                            else
                            {
                                range.NumberFormat = "0"; // Format as an int.
                            }
                        }
                        else
                        {
                            range.NumberFormat = "@"; // Format as text.
                        }
                    }

                    ws.Cells[i, j] = tmp;
                }
            }
        }

        if (includeColumnHeaders)
        {
            Excel.Range headerRow = (Excel.Range)ws.Rows[columnHeaderRow];
            headerRow.Insert();
            foreach (DataColumn column in dt.Columns)
            {
                ws.Cells[columnHeaderRow, column.Ordinal + 1] = column.ColumnName;
            }
        }
        if (autofit) ws.Columns.AutoFit();
        if (landscape) ws.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
        if (show) app.Visible = true;
    }
    public void print(int orient, bool printSilent = false, string printerName = "")
    {
        string originalPrinter = "";
        if (printerName != "")
        {
            //This changes the default printer on the computer so we will want to set it back after we print our document.
            originalPrinter = app.ActivePrinter;
            app.ActivePrinter = printerName;
        }
        if (printSilent)
        {
            if (orient == 1)
                ws.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            ws.PrintOutEx();
        }
        else
        {
            app.Visible = true;
            if (orient == 1)
                ws.PageSetup.Orientation = Excel.XlPageOrientation.xlLandscape;
            ws.PrintOut(blank, blank, blank, true, blank, blank, blank, blank);
            app.Visible = false;
            app.ActiveWorkbook.Saved = true;
        }
        if (printerName != "")
            app.ActivePrinter = originalPrinter;
    }
    public void save(string path, bool isPDF = false, bool openPdf = true)
    {
        bool origDisplayAlerts = app.DisplayAlerts;
        app.DisplayAlerts = false;
        if (isPDF)
        {
            wb.ExportAsFixedFormat(Excel.XlFixedFormatType.xlTypePDF, path, Excel.XlFixedFormatQuality.xlQualityStandard, true, true, Type.Missing, Type.Missing, openPdf, Type.Missing);
        }
        else
        {
            wb.SaveAs(path, blank, blank, blank, blank, blank, Excel.XlSaveAsAccessMode.xlExclusive, blank, blank, blank, blank);
            app.DisplayAlerts = origDisplayAlerts;
        }
    }
    //public void SaveAsCSV(string path)
    //{
    //    bool origDisplayAlerts = app.DisplayAlerts;
    //    app.DisplayAlerts = false;
    //    wb.SaveAs(path, Excel.XlFileFormat.xlCSV, blank, blank, blank, blank, Excel.XlSaveAsAccessMode.xlExclusive, blank, blank, blank, blank);
    //    app.DisplayAlerts = origDisplayAlerts;
    //}
    public void clearrows(int startrow, int endrow)
    {
        Excel.Range range = ws.Range["A" + startrow, "Z" + endrow];
        range.Clear();
        range.ClearContents();
        range = ws.Range["A1", "A1"];
        range.Select();
    }
    public string getcell(string cell)
    {
        return ws.Range[cell, cell].Text.ToString();
    }
    //public void setcell(int i, int j, string val)
    //{
    //    ws.Cells[i, j] = val;
    //}
    //public void gridlines()
    //{
    //    ws.PageSetup.PrintGridlines = true;
    //}
    public void addimage(int i, int j, string file)
    {
        Image oImage = Image.FromFile(file);
        Excel.Range range = (Excel.Range)ws.Cells[i, j];
        //System.Windows.Forms.Clipboard.SetDataObject(oImage, true); 
        //range.set_Item(i, j, oImage);
        range.Select();
        ws.PasteSpecial(oImage, blank, blank, blank, blank, blank);
        //or
        //s.Shapes.AddPicture(file, Microsoft.Office.Core.MsoTriState.msoFalse,Microsoft.Office.Core.MsoTriState.msoCTrue, 10, 10, 100, 100);
    }
    public void close()
    {
        if (app != null)
        {
            app.DisplayAlerts = false; // Don't prompt the user to save on exit.
            app.Quit();

            ws = null;
            wb = null;
            app = null;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            Thread.Sleep(100); // Give Excel time to quit in case it's being stubborn.
        }
    }

    /// <summary>
    /// By default, returns a DataTable containing the data from the first worksheet encountered when worksheets are ordered by name.
    /// There is no way to grab a worksheet by its order of appearance in Excel when using OLEDB.
    /// To get a specific worksheet, you can specify either the worksheetName or the requiredColumns parameter.
    /// </summary>
    /// <param name="file">The full path to the file.</param>
    /// <param name="hasHeaders">When true, the first row is added to DataTable.Columns.</param>
    /// <param name="startingRow">The row number of the first row containing data to parse.</param>
    /// <param name="worksheetName">The name of the sheet to parse. Returns null when a worksheet of this name does not exist. When not specificed, the sheet used is the first encountered when sheets are ordered by name.</param>
    /// <param name="requiredColumns">A collection of column names that must appear in the sheet being parsed. Cycles through each sheet when worksheetName is not specified. If no matching sheet is found, a null DataTable is returned.</param>
    /// <returns></returns>
    public static DataTable getDataTableFromExcel(string file, bool hasHeaders, int startingRow = 1, string worksheetName = null, string[] requiredColumns = null)
    {
        DataTable dt = null;
        string fileExtension = Path.GetExtension(file);
        var conn = new OleDbConnection(fileExtension.Equals(".xlsx", StringComparison.OrdinalIgnoreCase) || fileExtension.Equals(".xlsm", StringComparison.OrdinalIgnoreCase)
            ? $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={file};Extended Properties=\"Excel 12.0;HDR={(hasHeaders ? "Yes" : "No")};IMEX=1\""
            : $"Provider=Microsoft.Jet.OLEDB.4.0;Data Source={file};Extended Properties=\"Excel 8.0;HDR={(hasHeaders ? "Yes" : "No")};IMEX=1\"");
        conn.Open();
        DataTable schemaTable = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });

        if (schemaTable != null)
        {
            if (worksheetName == null)
            {
                foreach (DataRow row in schemaTable.Rows)
                {
                    string sheet = row["TABLE_NAME"].ToString();
                    dt = getDataTableFromExcelSheet(sheet, conn, hasHeaders, startingRow, requiredColumns);
                    if (dt != null) break;
                }
            }
            else
            {
                string sheet = null;
                foreach (DataRow row in schemaTable.Rows)
                {
                    sheet = row["TABLE_NAME"].ToString();
                    if (sheet == "'" + worksheetName + "$'" || sheet == worksheetName + "$")
                    {
                        break;
                    }
                    sheet = null;
                }
                dt = sheet == null ? null : getDataTableFromExcelSheet(sheet, conn, hasHeaders, startingRow, requiredColumns);
            }
            schemaTable.Dispose();
        }

        conn.Close();
        return dt;
    }
    private static DataTable getDataTableFromExcelSheet(string sheet, OleDbConnection conn, bool hasHeaders, int startingRow, string[] requiredColumns)
    {
        string query = "SELECT * FROM [" + sheet.Replace("'", "") + "A" + startingRow + ":ZZ]";
        var dt = new DataTable();
        var da = new OleDbDataAdapter(query, conn);
        dt.Locale = CultureInfo.CurrentCulture;
        da.Fill(dt);
        da.Dispose();

        if (hasHeaders)
        {
            foreach (DataColumn col in dt.Columns)
            {
                col.ColumnName = col.ColumnName.Trim();
            }
        }

        if (requiredColumns != null)
        {
            foreach (string column in requiredColumns)
            {
                if (!dt.Columns.Contains(column))
                {
                    dt = null;
                    break;
                }
            }
        }

        return dt;
    }

    public void Dispose()
    {
        try
        {
            this.close();
        }
        catch (Exception)
        {

            throw;
        }
    }
}

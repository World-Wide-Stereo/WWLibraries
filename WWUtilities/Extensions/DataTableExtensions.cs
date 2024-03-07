using System.Data;
using System.Diagnostics;
using System.Text;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class DataTableExtensions
    {
        private static string GetFlatFileColumnHeadersString(this DataTable dt, char separatorChar, bool useQuotedColumns)
        {
            const string quote = "\"";
            var csvColumnHeaders = new StringBuilder();
            foreach (DataColumn dtColumn in dt.Columns)
            {
                if (csvColumnHeaders.Length > 0) csvColumnHeaders.Append(separatorChar);
                if (useQuotedColumns)
                {
                    csvColumnHeaders.Append(quote).Append(dtColumn.ColumnName.Trim().Replace(quote, "\"\"")).Append(quote); //Escape commas and quotation marks
                }
                else
                {
                    csvColumnHeaders.Append(dtColumn.ColumnName.Trim());
                }
            }
            return csvColumnHeaders.ToString();
        }
        private static string GetFlatFileDataRowString(this DataRow row, char separatorChar, bool useQuotedColumns)
        {
            const string quote = "\"";
            var csvRow = new StringBuilder();
            bool appendSeparatorChar = false;
            foreach (var dtColumn in row.ItemArray)
            {
                if (appendSeparatorChar) csvRow.Append(separatorChar);
                if (useQuotedColumns)
                {
                    csvRow.Append(quote).Append(dtColumn.ToString().Trim().Replace(quote, "\"\"")).Append(quote); //Escape commas and quotation marks
                }
                else
                {
                    csvRow.Append(dtColumn.ToString().Trim());
                }
                appendSeparatorChar = true;
            }
            return csvRow.ToString();
        }
        public static string ToFlatFileString(this DataTable dt, char separatorChar = ',', bool useQuotedColumns = true, bool includeHeaders = true)
        {
            var sb = new StringBuilder();
            if (includeHeaders)
            {
                sb.AppendLine(dt.GetFlatFileColumnHeadersString(separatorChar, useQuotedColumns));
            }
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine(row.GetFlatFileDataRowString(separatorChar, useQuotedColumns));
            }
            return sb.ToString();
        }

        public static string ToHTMLTable(this DataTable dt)
        {
            StringBuilder htmlBuilder = new StringBuilder("<table>");
            //add header row
            htmlBuilder.Append("<tr>");
            foreach (DataColumn column in dt.Columns)
            {
                htmlBuilder.Append($"<td>{column.ColumnName}</td>");
            }
            htmlBuilder.Append("</tr>");

            //add rows
            foreach (DataRow row in dt.Rows)
            {
                htmlBuilder.Append("<tr>");
                foreach (object value in row.ItemArray)
                {
                    htmlBuilder.Append($"<td>{value}</td>");
                }
                htmlBuilder.Append("</tr>");
            }
            htmlBuilder.Append("</table>");
            return htmlBuilder.ToString();
        }
    }
}

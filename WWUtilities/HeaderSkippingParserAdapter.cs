using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenericParsing;
using System.Data;

namespace ww.Utilities
{
	public class HeaderSkippingParserAdapter : GenericParserAdapter
	{

/*/		public HeaderSkippingParserAdapter(string strFileName) : base(strFileName) { }

		public int SkipStartingHeaderRows { get; set; }

		public new DataTable GetDataTable()
		{
			for (int i = 0; i < SkipStartingHeaderRows; i++)
			{
				this.Read();
			}
			return base.GetDataTable();
		}

/*/		public HeaderSkippingParserAdapter(string strFileName)
		{
			this.SetDataSource(strFileName);
		}

		public int SkipStartingHeaderRows { get; set; }

		public new DataTable GetDataTable()
		{
			DataRow drRow;
			DataTable dtData;
			int intCreatedColumns;
		    bool hasHeaders = false;

            //FirstRowHasHeader must be false so that this.Read() will be able to read the first row in the table
		    if (this.FirstRowHasHeader)
		    {
		        this.FirstRowHasHeader = false;
		        hasHeaders = true;
		    }

			dtData = new DataTable();
			dtData.BeginLoadData();

			intCreatedColumns = 0;

			int lines = 0;
			while (this.Read())
			{
				if (lines == SkipStartingHeaderRows)
				{
				    for (int intColumnIndex = intCreatedColumns; intColumnIndex < this.m_lstData.Count; ++intColumnIndex, ++intCreatedColumns)
				    {
				        HeaderSkippingParserAdapter.AddColumnToTable(dtData, hasHeaders ? this.m_lstData[intColumnIndex].Trim() : null);
				    }
				}
				if (!this.IsCurrentRowEmpty || !this.SkipEmptyRows)
				{
                    if (lines > SkipStartingHeaderRows || (lines == SkipStartingHeaderRows && !hasHeaders))
					{
						drRow = dtData.NewRow();

						// Since we don't have to account for the row number, just place the value right into the data row.
						drRow.ItemArray = this.m_lstData.ToArray();
						dtData.Rows.Add(drRow);
					}
				}
				lines++;
			}


			dtData.EndLoadData();

			return dtData;
		}

		#region Yanked from GenericParserAdapter Source
		private const string FILE_LINE_NUMBER = "FileLineNumber";

		private static void AddColumnToTable(DataTable dtData, string strColumnName)
		{
			if (strColumnName != null)
			{
				if (dtData.Columns[strColumnName] == null)
					dtData.Columns.Add(strColumnName);
				else
				{
					string strNewColumnName;
					int intCount = 0;

					// Looks like we need to generate a new column name.
					do
					{
						strNewColumnName = $"{strColumnName}{++intCount}";
					}
					while (dtData.Columns[strNewColumnName] != null);

					dtData.Columns.Add(strNewColumnName);
				}
			}
			else
				dtData.Columns.Add();
		}
		#endregion
/**/
	}
}

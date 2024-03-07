using GenericParsing;
using System.Data;

namespace ww.Utilities
{
	public class PayPalParserAdapter : GenericParserAdapter
	{
		public string SettlementID { get; private set; }

		public PayPalParserAdapter(string strFileName)
		{
			this.SetDataSource(strFileName);
		}

		public new DataTable GetDataTable()
		{
			DataRow drRow;
			DataTable dtData;
			int intCreatedColumns;

			dtData = new DataTable();
			dtData.BeginLoadData();

			intCreatedColumns = 0;

			while (this.Read())
			{
				if (this.m_lstData[0] == "SH")
				{
					this.SettlementID = m_lstData[3].ToString();
				}

				// See if we have the appropriate number of columns.
				if (this.m_lstData[0] == "CH")
				{
					for (int intColumnIndex = intCreatedColumns; intColumnIndex < this.m_lstData.Count; ++intColumnIndex, ++intCreatedColumns)
						PayPalParserAdapter.AddColumnToTable(dtData, this.m_lstData[intColumnIndex]);
				}

				if (!this.IsCurrentRowEmpty || !this.SkipEmptyRows)
				{
					if (this.m_lstData[0] == "SB")
					{
						drRow = dtData.NewRow();

						// Since we don't have to account for the row number, just place the value right into the data row.
						drRow.ItemArray = this.m_lstData.ToArray();
						dtData.Rows.Add(drRow);
					}
				}
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
	}
}

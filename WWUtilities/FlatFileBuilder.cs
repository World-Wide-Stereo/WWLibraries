using System.Collections.Generic;
using System.Data;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
	public class FlatFileBuilder : DataTable
	{
		private Dictionary<string, int> rowIDs = new Dictionary<string, int>();


		public void AddColumn(string colName)
		{
			this.Columns.Add(colName);
		}

		public void AddColumns(params string[] colNames)
		{
			foreach (string col in colNames)
			{
				AddColumn(col);
			}
		}

		public bool RowExists(string rowKey)
		{
			return rowIDs.ContainsKey(rowKey);
		}

		public string ToString(char separatorChar = ',', bool useQuotedColumns = true, bool includeHeaders = true)
		{
			return this.ToFlatFileString(separatorChar: separatorChar, useQuotedColumns: useQuotedColumns, includeHeaders: includeHeaders);
		}

		#region DataTable Overrides
		protected override DataRow NewRowFromBuilder(DataRowBuilder builder)
		{
			return new FlatFileRow(builder);
		}

		public FlatFileRow this[string rowKey]
		{
			get
			{
				if (rowIDs.ContainsKey(rowKey))
				{
					return (FlatFileRow)this.Rows[rowIDs[rowKey]];
				}

				FlatFileRow newRow = (FlatFileRow)this.NewRow();
				this.Rows.Add(newRow);
				rowIDs.Add(rowKey, this.Rows.Count - 1);
				return newRow;
			}
		}

		public class FlatFileRow : DataRow
		{
			internal FlatFileRow(DataRowBuilder builder) : base(builder) { }
		}
		#endregion
	}
}

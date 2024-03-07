using System.Data;

namespace ww.Tables
{
    public enum SqlServerCustomAutoNumberType
    {
        ExampleTable = 1,
    }

    internal static class SqlServerCustomAutoNumber
    {
        internal static int GetNextCustomAutoNumber(SqlServerConnection connection, SqlServerCustomAutoNumberType autoNumberType)
        {
            int num = 0;
            bool ok2cont = true;
            do
            {
                try
                {
                    var data = connection.GetDataAndLock("select * from CustomAutoNumber");
                    foreach (DataRow row in data.Table.Rows)
                    {
                        switch (autoNumberType)
                        {
                            case SqlServerCustomAutoNumberType.ExampleTable:
                                num = int.Parse(row["ExampleTable"].ToString());
                                row["ExampleTable"] = num + 1;
                                break;
                        }
                    }
                    // It's safe to use the function below because we are not adding or removing any rows from data.Table.
                    // Using this function is necessary to avoid concurrency violations.
                    connection.UpdateDataAndUnlock(data);
                    ok2cont = false;
                }
                catch { }
            } while (ok2cont);
            return num;
        }
    }
}

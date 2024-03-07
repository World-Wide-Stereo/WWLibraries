using System;
using System.Data;

namespace ww.Tables
{
    public enum AdvantageCustomAutoNumberType
    {
        ExampleTable = 1,
    }

    internal class AdvantageCustomAutoNumber
    {
        internal static int GetNextCustomAutoNumber(AdvantageConnection connection, AdvantageCustomAutoNumberType autoNumberType)
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
                            case AdvantageCustomAutoNumberType.ExampleTable:
                                num = Int32.Parse(row["ExampleTable"].ToString());
                                row["ExampleTable"] = num + 1;
                                break;
                        }
                    }
                    // It's safe to use the function below because we are not adding or removing any rows from data.Table.
                    // Using this function is necessary to avoid concurrency violations.
                    connection.UpdateDataAndUnlock_DatabaseTable(data);
                    ok2cont = false;
                }
                catch { }
            } while (ok2cont);
            return num;
        }
    }
}

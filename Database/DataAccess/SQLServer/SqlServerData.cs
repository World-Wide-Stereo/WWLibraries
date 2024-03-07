using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ww.Tables
{
    public class SqlServerData : DatabaseData
    {
        #region Fields and Properties
        internal SqlDataAdapter Adapter;
        internal SqlTransaction Transaction;
        #endregion

        #region Constructors
        public SqlServerData(SqlServerConnection connection, string query, List<IDbDataParameter> parameters)
        {
            this.Connection = connection;
            this.Query = query;
            this.Parameters = parameters;
        }
        #endregion

        #region Public Functions
        public override void UnlockWithoutUpdatingData()
        {
            ((SqlServerConnection)this.Connection).UnlockWithoutUpdatingData(this);
        }

        public override void UpdateDataAndUnlock()
        {
            ((SqlServerConnection)this.Connection).UpdateDataAndUnlock(this);
        }

        public override void DeleteLockedData()
        {
            ((SqlServerConnection)this.Connection).DeleteLockedData(this);
        }
        #endregion
    }
}

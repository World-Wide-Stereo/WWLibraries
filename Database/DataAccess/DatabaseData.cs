using System.Collections.Generic;
using System.Data;

namespace ww.Tables
{
    public abstract class DatabaseData
    {
        #region Fields and Properties
        public DatabaseConnection Connection { get; internal set; }
        public DataTable Table { get; internal set; }
        internal string Query { get; set; }
        internal List<IDbDataParameter> Parameters { get; set; }
        public bool IsLocked { get; internal set; }
        #endregion

        #region Public Functions
        public abstract void UnlockWithoutUpdatingData();
        public abstract void UpdateDataAndUnlock();
        public abstract void DeleteLockedData();
        #endregion
    }
}

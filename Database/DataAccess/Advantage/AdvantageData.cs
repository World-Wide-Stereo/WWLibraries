using System.Collections.Generic;
using System.Data;
using Advantage.Data.Provider;

namespace ww.Tables
{
    public class AdvantageData : DatabaseData
    {
        #region Fields and Properties
        internal AdsExtendedReader Reader;
        #endregion

        #region Constructors
        public AdvantageData(AdvantageConnection connection, string query, List<IDbDataParameter> parameters)
        {
            this.Connection = connection;
            this.Query = query;
            this.Parameters = parameters;
        }
        #endregion

        #region Public Functions
        public override void UnlockWithoutUpdatingData()
        {
            ((AdvantageConnection)this.Connection).UnlockWithoutUpdatingData(this);
        }

        public override void UpdateDataAndUnlock()
        {
            ((AdvantageConnection)this.Connection).UpdateDataAndUnlock(this);
        }

        public override void DeleteLockedData()
        {
            ((AdvantageConnection)this.Connection).DeleteLockedData(this);
        }
        #endregion
    }
}

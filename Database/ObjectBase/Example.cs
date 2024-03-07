using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Linq;

namespace ww.Tables
{
    [DatabaseTable("tablename", Database.DatabaseType.SqlServer)]
    public class Example : DatabaseTable
    {
        #region Fields
        //[Database("columnname", IsPrimaryKey = true, CustomAutoNumberType = (int)SqlServerCustomAutoNumberType.Example)]
        [Database("columnname", IsPrimaryKey = true, IsAutoNumber = true)]
        public int ID = NextCustomAutoNumber;
        [Database("column1", IsSecondaryKey = true)]
        public string Column1;
        [Database("column2")]
        public string Column2;
        #endregion

        #region Constructors
        public Example(int id)
        {
            this.ID = id;
            Initialize();
        }
        public Example(string column1)
        {
            this.Column1 = column1;
            UseSecondaryKey = true;
            Initialize();
        }
        public Example(DataRow row)
        {
            Initialize(row);
        }
        #endregion

        #region Public Functions
        public static IEnumerable<Example> GetAll()
        {
            return Global.SqlConn.GetData("select * from tablename").AsEnumerable().Select(x => new Example(x));
        }
        #endregion

        #region Protected Functions
        protected override void DataRequirementsMetForUpdate(HashSet<string> fieldsToSkip = null)
        {
            // Prevent updating under certain circumstances by throwing a DataRequirementException with a meaningful message.
        }

        protected override void PreUpdate()
        {
            // Set fields based on calculations that are the same for every record.
        }

        protected override void PostUpdate()
        {
            // Update other tables when critical data that is involved in calculating a field on the other table has changed.
        }
        #endregion
    }
}

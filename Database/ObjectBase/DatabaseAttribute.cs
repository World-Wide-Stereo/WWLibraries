using System;

namespace ww.Tables
{
    public abstract class DatabaseAttributeBase : Attribute
    {
        public bool ReadFromDatabase = true;
        public bool SaveToDatabase = true;
        public bool IsPrimaryKey = false;
        public bool IsSecondaryKey = false;
        public bool ConvertToNull = false;
        public SqlStringDataType sqlStringDataType;
        public int TruncateStringAt = -1;

        protected Database.DataType type = Database.DataType.Unknown;

        protected string databaseColumn;

        #region Constructors
        protected DatabaseAttributeBase()
        {
        }
        protected DatabaseAttributeBase(string databaseColumn)
        {
            this.databaseColumn = databaseColumn;
        }
        protected DatabaseAttributeBase(string databaseColumn, Database.DataType type)
        {
            this.databaseColumn = databaseColumn;
            this.type = type;
        }
        #endregion

        #region Properties
        public string DatabaseColumn
        {
            get { return this.databaseColumn; }
        }
        #endregion

        public abstract bool IsKey(bool useSecondaryKey);
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class DatabaseAttribute : DatabaseAttributeBase
    {
        public bool IsAutoNumber = false;
        public int CustomAutoNumberType;

        #region Constructors
        public DatabaseAttribute()
        {
        }
        public DatabaseAttribute(string databaseColumn) : base(databaseColumn)
        {
        }
        public DatabaseAttribute(string databaseColumn, Database.DataType type) : base(databaseColumn, type)
        {
        }
        #endregion

        #region Properties
        public Database.DataType Type
        {
            get { return this.type; }
        }
        #endregion

        public override bool IsKey(bool useSecondaryKey)
        {
            return useSecondaryKey ? this.IsSecondaryKey : this.IsPrimaryKey;
        }
    }
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class WebAttribute : DatabaseAttributeBase
    {
        #region Constructors
        public WebAttribute()
        {
        }
        public WebAttribute(string databaseColumn) : base(databaseColumn)
        {
        }
        public WebAttribute(string databaseColumn, Database.DataType type) : base(databaseColumn, type)
        {
        }
        #endregion

        public override bool IsKey(bool useSecondaryKey)
        {
            // Web tables should always use the primary key.
            return this.IsPrimaryKey;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class DatabaseTableAttribute : Attribute
    {
        public bool ThrowWebErrors = false;

        private string _tableName;
        public string TableName
        {
            get { return _tableName; } 
        }

        private Database.DatabaseType _databaseType;
        public Database.DatabaseType DatabaseType
        {
            get { return _databaseType; }
        }

        private bool _isDatabaseTableDetail;
        public bool IsDatabaseTableDetail
        {
            get { return _isDatabaseTableDetail; }
        }

        public DatabaseTableAttribute(string tableName, Database.DatabaseType databaseType, bool isDatabaseTableDetail = false)
        {
            this._tableName = tableName;
            this._databaseType = databaseType;
            this._isDatabaseTableDetail = isDatabaseTableDetail;
        }
    }

    public enum SqlStringDataType
    {
        Default,
        Char,
        NChar,
        VarChar,
        NVarChar
    }
}

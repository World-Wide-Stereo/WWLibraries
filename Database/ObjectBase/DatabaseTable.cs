using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using ww.Utilities;
using ww.Utilities.Extensions;

namespace ww.Tables
{
    public abstract partial class DatabaseTable : IDisposable
    {
        /// <summary>
        /// Magic number to indicate that we need a new autonumber when this gets saved.
        /// </summary>
        public const int NextCustomAutoNumber = -500;

        private bool Initialized;
        protected bool UseSecondaryKey = false;
        public bool ExistsInDatabase { get; private set; }
        public bool IsLocked { get { return DatabaseData != null && DatabaseData.IsLocked; } }

        private DatabaseData DatabaseData;

        private string _tableName;
        public string TableName
        {
            get
            {
                if (_tableName == null)
                {
                    var atrb = GetTableAttribute();
                    if (atrb == null) throw new NotSupportedException("No table mapping for class " + this.Type);
                    _tableName = atrb.TableName;
                }
                return _tableName;
            }
        }

        private Database.DatabaseType? _databaseType;
        private Database.DatabaseType DatabaseType
        {
            get
            {
                if (_databaseType == null)
                {
                    _databaseType = GetTableAttribute().DatabaseType;
                }
                return _databaseType.Value;
            }
        }

        private NotSupportedException UnsupportedDatabaseTypeException
        {
            get { return new NotSupportedException("Unsupported database type for table " + this.TableName); }
        }

        private Type _type;
        private Type Type
        {
            get { return _type ?? (_type = this.GetType()); }
        }

        private Type[] _interfaces;
        private Type[] Interfaces
        {
            get { return _interfaces ?? (_interfaces = this.Type.GetInterfaces()); }
        }

        private bool? _isDatabaseTableDetail;
        internal bool IsDatabaseTableDetail
        {
            get
            {
                if (_isDatabaseTableDetail == null)
                {
                    _isDatabaseTableDetail = GetTableAttribute().IsDatabaseTableDetail;
                }
                return _isDatabaseTableDetail.Value;
            }
        }

        public bool IsEmpty
        {
            get
            {
                List<string> keyFieldNames = GetKeyMembers(useAllKeys: true).Select(x => x.Name).ToList();
                return GetAllFields().All(x =>
                {
                    if (keyFieldNames.Contains(x.Name)) return true; // Key fields will never be empty due to the constructor populating them.
                    Type type = x.GetUnderlyingType();
                    object currentValue = x.GetValue(this);
                    object defaultValue = type.GetDefault();
                    return (currentValue == null && defaultValue == null) || currentValue == null || currentValue.Equals(defaultValue) || (type == typeof(string) && currentValue.Equals(""));
                });
            }
        }

        private DatabaseConnection GetConnection()
        {
            switch (this.DatabaseType)
            {
                case Database.DatabaseType.Advantage:
                    return Global.AdsConn;
                case Database.DatabaseType.SqlServer:
                    return new SqlServerConnection();
                default:
                    throw UnsupportedDatabaseTypeException;
            }
        }

        /// <summary>
        /// Initializes the data object using previously-specified values for the key fields.
        /// </summary>
        protected void Initialize(bool lockRecord = false)
        {
            Initialize(GetDefaultData(lockRecord));

            // If we've locked the record, load details simply for the purpose of locking their records.
            if (lockRecord)
            {
                foreach (PropertyInfo pi in GetAllDetailProperties())
                {
                    dynamic member = pi.GetValue(this);
                    if (member != null)
                    {
                        member.Load();
                        member.PostLoad();
                    }
                }
            }
        }
        /// <summary>
        /// Initializes the data object using the first row of the provided data table.
        /// </summary>
        private void Initialize(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                Initialize(dt.Rows[0]);
            }
            else
            {
                // Change null strings to empty strings. This matches the behavior used when a record exists.
                foreach (MemberInfo mi in GetAllFields().Where(x => x.GetUnderlyingType() == typeof(string) && x.GetValue(this) == null))
                {
                    mi.SetValue(this, "");
                }

                InitializeLazyVars();
            }
        }
        /// <summary>
        /// Initializes the data object using a data row.
        /// </summary>
        protected void Initialize(DataRow row)
        {
            if (row != null)
            {
                ExistsInDatabase = true;
                foreach (MemberInfo mi in GetAllFields())
                {
                    DatabaseAttribute attr = GetAttribute(mi);
                    if (attr.ReadFromDatabase)
                    {
                        SetFieldValue(mi, row[attr.DatabaseColumn]);
                    }
                }
            }
            else
            {
                ExistsInDatabase = false;
            }

            InitializeLazyVars();
            // We don't need to do anything here because instantiating the class object already set up default values.
            foreach (MemberInfo mi in GetAllLazyMembers())
            {
                if (mi.GetValue(this) == null)
                {
                    throw new NotSupportedException("No initialization found for Lazy<T> " + mi.Name + " in class " + this.Type);
                }
            }

            Initialized = true;
        }

        protected virtual void InitializeLazyVars() { }

        /// <summary>
        /// Saves the data object to the database if the data requirements are met.
        /// </summary>
        public void Update([CallerFilePath] string callingFile = null, HashSet<string> dataRequirementFieldsToSkip = null)
        {
            if (IsDatabaseTableDetail && !Path.GetFileNameWithoutExtension(callingFile).EqualsAnyOf(nameof(DatabaseTable), nameof(DatabaseTableDetail), nameof(DatabaseTableDetailList<object, object>), nameof(DatabaseTableDetailDictionary<object, object, object>)))
            {
                throw new NotSupportedException("Updating is only allowed via the parent object.");
            }

            DataRequirementsMetForUpdate(dataRequirementFieldsToSkip);
            PreUpdate();
            DataTable dt = Initialized ? GetDefaultData() : GetNullData();
            if (dt.Rows.Count == 0) dt.Rows.Add(dt.NewRow());
            DataRow row = dt.Rows[0];
            MemberInfo autoNumberField = null;

            foreach (MemberInfo mi in GetAllFields())
            {
                DatabaseAttribute attr = GetAttribute(mi);
                if (attr.SaveToDatabase)
                {
                    SetCustomAutoNumber(mi, attr);
                    row[attr.DatabaseColumn] = GetValueForDatabase(mi, attr, out Type underlyingType);
                }
                if (attr.IsAutoNumber && autoNumberField == null)
                {
                    // While multiple fields in a class can be flagged with IsAutoNumber,
                    // only one field in a table can actually be an autonumber due to database restrictions.
                    autoNumberField = mi;
                }
            }

            int autoNumber = 0;
            if (IsLocked)
            {
                DatabaseData.Table = dt;
                try
                {
                    switch (this.DatabaseType)
                    {
                        case Database.DatabaseType.Advantage:
                            Global.AdsConn.UpdateDataAndUnlock_DatabaseTable((AdvantageData)DatabaseData);
                            break;
                        default:
                            this.DatabaseData.UpdateDataAndUnlock();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    var truncationEx = ex as IDatabaseDataTruncationException;
                    if (truncationEx != null)
                    {
                        Email.sendAlertEmail("Data Truncation Warning", "Increase the size of the database field or limit the size of textboxes saving to this field. Also ensure that this exception did not cause a crash.\n\nUser: " + Environment.UserName + "\nTable: " + TableName.ToUpper() + "\nKeys: " + GetKeyValues().Join(", ") + "\nColumn: " + truncationEx.ColumnName + "\nColumn Size: " + truncationEx.ColumnSize + "\nValue Size: " + truncationEx.ValueBeingTruncated.ToString().Length + "\nValue: " + truncationEx.ValueBeingTruncated);
                    }
                    throw;
                }
            }
            else
            {
                DatabaseConnection conn = GetConnection();
                try
                {
                    List<IDbDataParameter> parameters;
                    conn.UpdateData(GetDefaultDataQuery(conn, out parameters), dt, parameters, out autoNumber);
                }
                catch
                {
                    if (!conn.IsGlobal) conn.Dispose();
                    throw;
                }
                if (!conn.IsGlobal) conn.Dispose();
            }
            if (autoNumber != 0 && autoNumberField != null)
            {
                // Populate the field flagged with IsAutoNumber with the newly incremented number.
                autoNumberField.SetValue(this, autoNumber);
            }
            ExistsInDatabase = true;
            Initialized = true;

            // Reset _defaultDataQuery.
            // This is necessary in two scenarios. 1) The query contains an autonumber that was just incremented. 2) DefaultDataQuery was set to NullDataQuery.
            _defaultDataQuery = null;
            _defaultDataParameters = null;

            // Perform supplementary updates if this is not an DatabaseWebTable.
            // On DatabaseWebTables, the parent record must be entered into the database before the details, otherwise a foreign key will be violated.
            // Likewise, PostUpdate() may contain updates to records with a foreign key relationship to the class calling Update().
            // DatabaseWebTable.Update() takes care of the calls to PostUpdate() and UpdateDetails().
            if (this.Interfaces.All(x => x != typeof(IDatabaseWebTable)))
            {
                PostUpdate();
                UpdateDetails();
            }

            this.Dispose();
        }

        protected void UpdateDetails()
        {
            // If this table has the IDetail interface...
            if (this.Interfaces.Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDetail<>)))
            {
                var detail = (IDetail)this;

                // If details are actually loaded...
                if (detail.hasLoadedDetails())
                {
                    // Save the details.
                    detail.saveDetails();
                }
            }

            foreach (PropertyInfo pi in GetAllDetailProperties())
            {
                object member = pi.GetValue(this);
                if (member != null)
                {
                    var detail = (DatabaseTableDetail)member;
                    if (detail.IsLoaded)
                    {
                        detail.Update();
                    }
                }
            }
        }

        public void Delete([CallerFilePath] string callingFile = null, HashSet<string> fieldsToSkip = null)
        {
            if (IsDatabaseTableDetail && !Path.GetFileNameWithoutExtension(callingFile).EqualsAnyOf(nameof(DatabaseTable), nameof(DatabaseTableDetail), nameof(DatabaseTableDetailList<object, object>), nameof(DatabaseTableDetailDictionary<object, object, object>)))
            {
                throw new NotSupportedException("Deleting is only allowed via the parent object.");
            }

            if (ExistsInDatabase)
            {
                DataRequirementsMetForDelete(fieldsToSkip);
                PreDelete();

                if (!IsLocked)
                {
                    GetDefaultData(true); // Populates DatabaseData with the locked record.
                }

                // Perform supplementary deletes if this is not an DatabaseWebTable.
                // On DatabaseWebTables, the details must be removed from the database before the parent record, otherwise a foreign key will be violated.
                // DatabaseWebTable.Delete() takes care of the call to DeleteDetails().
                if (this.Interfaces.All(x => x != typeof(IDatabaseWebTable)))
                {
                    DeleteDetails();
                }

                if (DatabaseData.Table.Rows.Count > 0)
                {
                    switch (this.DatabaseType)
                    {
                        case Database.DatabaseType.Advantage:
                            Global.AdsConn.DeleteLockedData_DatabaseTable((AdvantageData)DatabaseData);
                            break;
                        default:
                            this.DatabaseData.DeleteLockedData();
                            break;
                    }
                }
                ExistsInDatabase = false;

                PostDelete();
            }
        }

        protected void DeleteDetails()
        {
            foreach (PropertyInfo pi in GetAllDetailProperties())
            {
                dynamic member = pi.GetValue(this);
                if (member != null)
                {
                    if (!member.IsLoaded)
                    {
                        member.Load();
                        member.PostLoad();
                    }
                    member.Delete();
                }
            }
        }

        public virtual void UnlockWithoutUpdating()
        {
            if (IsLocked)
            {
                foreach (PropertyInfo pi in GetAllDetailProperties())
                {
                    object member = pi.GetValue(this);
                    if (member != null)
                    {
                        var detail = (DatabaseTableDetail)member;
                        if (detail.IsLoaded)
                        {
                            detail.UnlockWithoutUpdating();
                        }
                    }
                }

                switch (this.DatabaseType)
                {
                    case Database.DatabaseType.Advantage:
                        Global.AdsConn.UnlockWithoutUpdatingData((AdvantageData)DatabaseData);
                        break;
                    default:
                        this.DatabaseData.UnlockWithoutUpdatingData();
                        break;
                }
            }

            this.Dispose();
        }

        protected DataRow ConvertToDataRow()
        {
            DataTable dt = GetDefaultData();
            if (dt.Rows.Count == 0) dt.Rows.Add(dt.NewRow());
            DataRow row = dt.Rows[0];

            return ConvertToDataRow(row);
        }
        protected DataRow ConvertToDataRow(DataRow oldRow)
        {
            foreach (MemberInfo mi in GetAllFields())
            {
                DatabaseAttribute attr = GetAttribute(mi);
                if (attr.SaveToDatabase)
                {
                    SetCustomAutoNumber(mi, attr);
                    oldRow[attr.DatabaseColumn] = GetValueForDatabase(mi, attr);
                }
            }

            return oldRow;
        }

        public void SetCustomAutoNumbers()
        {
            foreach (MemberInfo mi in GetAllFields())
            {
                DatabaseAttribute attr = GetAttribute(mi);
                SetCustomAutoNumber(mi, attr);
            }
        }
        private void SetCustomAutoNumber(MemberInfo mi, DatabaseAttribute attr)
        {
            if (attr.CustomAutoNumberType != 0 && GetValueForDatabase(mi, attr).ToInt() == NextCustomAutoNumber)
            {
                DatabaseConnection conn = GetConnection();
                try
                {
                    SetFieldValue(mi, conn.GetNextCustomAutoNumber(attr.CustomAutoNumberType));
                }
                catch
                {
                    if (!conn.IsGlobal) conn.Dispose();
                    throw;
                }
                if (!conn.IsGlobal) conn.Dispose();
            }
        }

        public IEnumerable<MemberInfo> GetKeyMembers(bool useAllKeys = false)
        {
            return GetAllFields().Where(x => {
                DatabaseAttribute attr = GetAttribute(x);
                return useAllKeys ? attr.IsPrimaryKey || attr.IsSecondaryKey : attr.IsKey(UseSecondaryKey);
                });
        }
        /// <summary>
        /// Return the values for all the key fields of this object.
        /// </summary>
        public IEnumerable<object> GetKeyValues()
        {
            return GetKeyMembers().Select(x => x.GetValue(this));
        }
        public static bool AreKeyValuesEqual(List<object> keyValues, List<object> origKeyValues)
        {
            for (int i = 0; i < keyValues.Count; i++)
            {
                if (keyValues[i] == null)
                {
                    if (origKeyValues[i] != null)
                    {
                        return false;
                    }
                }
                else if (!keyValues[i].Equals(origKeyValues[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Deletes records removed from a list. Must pass in the current list, after all operations have been carried out, and the original list.
        /// </summary>
        public static void DeleteRemovedDatabaseTables(IEnumerable<DatabaseTable> currentDatabaseTables, IEnumerable<DatabaseTable> origDatabaseTables)
        {
            List<List<object>> keyValuesForCurrentDatabaseTables = currentDatabaseTables.Select(x => x.GetKeyValues().ToList()).ToList(); // One list of key values for each DatabaseTable.
            foreach (DatabaseTable origDatabaseTable in origDatabaseTables)
            {
                List<object> keyValuesForOrigDatabaseTable = origDatabaseTable.GetKeyValues().ToList();
                // If none of the currentDatabaseTables have the same key values as this origDatabaseTable...
                if (keyValuesForCurrentDatabaseTables.All(x => !AreKeyValuesEqual(x, keyValuesForOrigDatabaseTable)))
                {
                    origDatabaseTable.Delete();
                }
            }
        }

        /// <summary>
        /// Override this in child classes to specify what is valid to save to the database. Called by Update().
        /// </summary>
        protected virtual void DataRequirementsMetForUpdate(HashSet<string> fieldsToSkip = null) { }

        /// <summary>
        /// Returns the first exception that DataRequirementsMetForUpdate() will throw, if any.
        /// This should only be called in cases where other records are updated based off of changes to the first.
        /// In such a scenario, an exception generated on Update() would leave the other records in a disagreeable state with the first.
        /// </summary>
        public DataRequirementException GetDataRequirementExceptionForUpdate(HashSet<string> fieldsToSkip = null)
        {
            try
            {
                DataRequirementsMetForUpdate(fieldsToSkip);
            }
            catch (DataRequirementException ex)
            {
                return ex;
            }
            return null;
        }

        /// <summary>
        /// A function to do something before an update, if overridden in a child class.
        /// </summary>
        protected virtual void PreUpdate() { }

        /// <summary>
        /// A function to do something after an update, if overridden in a child class.
        /// </summary>
        protected virtual void PostUpdate() { }

        /// <summary>
        /// Override this in child classes to specify what is valid to delete from the database. Called by Delete().
        /// </summary>
        protected virtual void DataRequirementsMetForDelete(HashSet<string> fieldsToSkip = null) { }

        /// <summary>
        /// Returns the first exception that DataRequirementsMetForDelete() will throw, if any.
        /// This should only be called in cases where other records are deleted based off of changes to the first.
        /// In such a scenario, an exception generated on Delete() would leave the other records in a disagreeable state with the first.
        /// </summary>
        public DataRequirementException GetDataRequirementExceptionForDelete(HashSet<string> fieldsToSkip = null)
        {
            try
            {
                DataRequirementsMetForDelete(fieldsToSkip);
            }
            catch (DataRequirementException ex)
            {
                return ex;
            }
            return null;
        }

        /// <summary>
        /// A function to do something before a delete, if overridden in a child class.
        /// </summary>
        protected virtual void PreDelete() { }

        /// <summary>
        /// A function to do something after a delete, if overridden in a child class.
        /// </summary>
        protected virtual void PostDelete() { }

        private string _defaultDataQuery;
        private List<IDbDataParameter> _defaultDataParameters;
        /// <summary>
        /// Gets a string representing the default query for this object. The query is built based on the key.
        /// Constructors using a secondary key must set UseSecondaryKey to true.
        /// </summary>
        // This must be a lazy-loaded property to ensure that the loaded record is the same one that will be updated.
        // When it was a function, it was possible to change one of the key fields and end up updating a different pre-existing record.
        protected string GetDefaultDataQuery(DatabaseConnection conn, out List<IDbDataParameter> parameters)
        {
            if (_defaultDataQuery == null)
            {
                _defaultDataParameters = new List<IDbDataParameter>();
                var query = new StringBuilder($"select * from [{TableName}]");
                bool hasWhere = false;
                int index = 1;
                foreach (MemberInfo mi in GetAllFields().Where(x => GetAttribute(x).IsKey(UseSecondaryKey)))
                {
                    if (!hasWhere)
                    {
                        query.Append(" where ");
                        hasWhere = true;
                    }
                    else
                    {
                        query.Append(" and ");
                    }

                    query.Append($"[{GetAttribute(mi).DatabaseColumn}] ");
                    if (mi.GetValue(this) == null)
                    {
                        query.Append("is null");
                    }
                    else
                    {
                        query.Append($"= {conn.NamedParameterChar}p{index}");
                        _defaultDataParameters.Add(conn.GetParameter($"p{index}", GetValueForQuery(mi), GetAttribute(mi).sqlStringDataType));
                        index++;
                    }
                }
                _defaultDataQuery = hasWhere ? query.ToString() : GetNullDataQuery();
            }
            parameters = _defaultDataParameters;
            return _defaultDataQuery;
        }

        private string _nullDataQuery;
        protected string GetNullDataQuery()
        {
            return _nullDataQuery ?? (_nullDataQuery = "select * from [" + TableName + "] where 0 = 1");
        }

        /// <summary>
        /// Returns the database-ready form of the data in a given field.
        /// </summary>
        /// <param name="mi">The MemberInfo for a field we want the value for.</param>
        protected object GetValueForQuery(MemberInfo mi)
        {
            Database.DataType type = GetDataType(mi);
            switch (type)
            {
                case Database.DataType.String:
                case Database.DataType.Character:
                    return mi.GetValue(this).ToString();
                case Database.DataType.Boolean:
                    return (bool)mi.GetValue(this);
                case Database.DataType.Decimal:
                    return (decimal)mi.GetValue(this);
                case Database.DataType.Integer:
                    return (int)mi.GetValue(this);
                case Database.DataType.Long:
                    return (long)mi.GetValue(this);
                case Database.DataType.Date:
                    return (DateTime)mi.GetValue(this);
                case Database.DataType.Time:
                    return (TimeSpan)mi.GetValue(this);
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }
        protected object GetValueForDatabase(MemberInfo mi, DatabaseAttribute attr)
        {
            return GetValueForDatabase(mi, attr, out _);
        }
        protected object GetValueForDatabase(MemberInfo mi, DatabaseAttribute attr, out Type underlyingType)
        {
            object value = mi.GetValue(this);

            if (attr.ConvertToNull && value == null)
            {
                underlyingType = null;
                return DBNull.Value;
            }

            Database.DataType type = GetDataType(mi, out underlyingType);
            switch (type)
            {
                case Database.DataType.String:
                case Database.DataType.Character:
                    // Trimming off whitespace so that string fields cannot end with odd characters such as '\n', causing queries matching on an exact string to fail to return that record.
                    if (value == null)
                    {
                        return "";
                    }
                    else
                    {
                        string returnVal = value.ToString().Trim();
                        return attr.TruncateStringAt > -1 && returnVal.Length > attr.TruncateStringAt
                            ? returnVal.Substring(0, attr.TruncateStringAt)
                            : returnVal;
                    }
                case Database.DataType.Boolean:
                    if (value == null)
                        return false;
                    else
                        return (bool)value;
                case Database.DataType.Decimal:
                    if (value == null)
                        return 0m;
                    else
                        return (decimal)value;
                case Database.DataType.Integer:
                    if (value == null)
                        return 0;
                    else
                        return (int)value;
                case Database.DataType.Long:
                    if (value == null)
                        return 0;
                    else
                        return (long)value;
                case Database.DataType.Date:
                    if (value == null || (DateTime)value == default(DateTime))
                        return DBNull.Value;
                    else
                        return (DateTime)mi.GetValue(this);
                case Database.DataType.Time:
                    if (value == null || (TimeSpan)value == default(TimeSpan))
                        return DBNull.Value;
                    else
                        return (TimeSpan)mi.GetValue(this);
                default:
                    throw new NotSupportedException(type.ToString());
            }
        }

        public IEnumerable<string> GetLockingUsers()
        {
            // Get locking users for this table.
            List<string> lockingUsers;
            DatabaseConnection conn = GetConnection();
            try
            {
                List<IDbDataParameter> parameters;
                lockingUsers = conn.GetLockingUsers(this.TableName, this.GetDefaultDataQuery(conn, out parameters), parameters).ToList();

                if (lockingUsers.Count == 0)
                {
                    // Get all fields and properties that are DatabaseTables.
                    IEnumerable<MemberInfo> subTableMembers =
                        this.Type
                        .GetFields()
                        .Where(x =>
                            x.FieldType.IsSubclassOf(typeof(DatabaseTable)) // Sub table.
                            || (x.FieldType.GetGenericArguments().Length > 0 && x.FieldType.GetGenericArguments()[0].IsSubclassOf(typeof(DatabaseTable))) // Collection of sub tables.
                            );
                    subTableMembers =
                        subTableMembers.Concat(
                        this.Type
                        .GetProperties()
                        .Where(x =>
                            x.PropertyType.IsSubclassOf(typeof(DatabaseTable)) // Sub table.
                            || (x.PropertyType.GetGenericArguments().Length > 0 && x.PropertyType.GetGenericArguments()[0].IsSubclassOf(typeof(DatabaseTable))) // Collection of sub tables.
                            )
                        );

                    // Get locking users for any tables potentially locked via fields or properties.
                    foreach (var memberInfo in subTableMembers)
                    {
                        object member = memberInfo.GetValue(this);
                        if (member != null)
                        {
                            if (memberInfo.GetUnderlyingType().IsSubclassOf(typeof(DatabaseTable)))
                            {
                                // Sub table.
                                var subTable = (DatabaseTable)member;
                                lockingUsers.AddRange(conn.GetLockingUsers(subTable.TableName, subTable.GetDefaultDataQuery(conn, out parameters), parameters));
                            }
                            else
                            {
                                // Collection of sub tables.
                                foreach (var subTable in (IEnumerable<DatabaseTable>)member)
                                {
                                    lockingUsers.AddRange(conn.GetLockingUsers(subTable.TableName, subTable.GetDefaultDataQuery(conn, out parameters), parameters));
                                }
                            }
                        }
                    }

                    // Remove duplicate user/computer combinations.
                    string prevLockingUser = null;
                    foreach (string lockingUser in lockingUsers.OrderBy(x => x).ToList())
                    {
                        if (lockingUser == prevLockingUser) lockingUsers.Remove(lockingUser);
                        else prevLockingUser = lockingUser;
                    }
                }
            }
            catch
            {
                if (!conn.IsGlobal) conn.Dispose();
                return new[] { "Unknown" };
            }
            if (!conn.IsGlobal) conn.Dispose();

            return lockingUsers;
        }

        public void GetDuplicateRecordKeyValues(out string databaseTableName, out List<string> primaryKeyFieldNames, out List<string> secondaryKeyFieldNames, out List<object> primaryKeyValues, out List<object> secondaryKeyValues, bool useDatabaseColumnNames = false)
        {
            databaseTableName = TableName;
            ReadOnlyCollection<MemberInfo> allFields = GetAllFields();

            var primaryKeyFields = allFields.Where(x => GetAttribute(x).IsPrimaryKey).ToList();
            GetKeyFieldNamesAndValues(useDatabaseColumnNames, primaryKeyFields, out primaryKeyFieldNames, out primaryKeyValues);

            var secondaryKeyFields = allFields.Where(x => GetAttribute(x).IsSecondaryKey).ToList();
            if (secondaryKeyFields.Count == 0 && primaryKeyFields.Count > 0 && primaryKeyFields.All(x => { var atrb = GetAttribute(x); return atrb.IsAutoNumber || atrb.CustomAutoNumberType > 0; }))
            {
                secondaryKeyFields = allFields.Where(x => !GetAttribute(x).IsPrimaryKey).ToList();
            }
            GetKeyFieldNamesAndValues(useDatabaseColumnNames, secondaryKeyFields, out secondaryKeyFieldNames, out secondaryKeyValues);
        }
        private void GetKeyFieldNamesAndValues(bool useDatabaseColumnNames, List<MemberInfo> keyFields, out List<string> keyFieldNames, out List<object> keyValues)
        {
            keyFieldNames = useDatabaseColumnNames
                ? keyFields.Select(x => GetAttribute(x).DatabaseColumn).ToList()
                : keyFields.Select(x => x.Name).ToList();
            keyValues = GetDuplicateRecordKeyValuesForKeyType(keyFields);
        }
        private List<object> GetDuplicateRecordKeyValuesForKeyType(List<MemberInfo> keyFields)
        {
            var keyValues = new List<object>();
            if (keyFields.Count > 0)
            {
                DatabaseConnection conn = GetConnection();
                DataTable dt;
                try
                {
                    dt = conn.GetData(GetDuplicateRecordQuery(keyFields));

                    if (!conn.IsGlobal) conn.Dispose();
                }
                catch (DatabaseException ex)
                {
                    if (!conn.IsGlobal) conn.Dispose();

                    if (ex.OrderByColumnFailed)
                    {
                        dt = new DataTable();
                    }
                    else
                    {
                        throw;
                    }
                }

                foreach (DataRow row in dt.Rows)
                {
                    foreach (var keyField in keyFields)
                    {
                        string dbColumnName = GetAttribute(keyField).DatabaseColumn;
                        keyValues.Add(keyField.GetUnderlyingType() == typeof(string) ? row[dbColumnName].ToString().Trim() : row[dbColumnName]);
                    }
                }
            }
            return keyValues;
        }
        private string GetDuplicateRecordQuery(List<MemberInfo> keyFields)
        {
            int numKeyFields = keyFields.Count;
            var query = new StringBuilder("select ");
            for (int i = 0; i < numKeyFields; i++)
            {
                query.Append("[").Append(GetAttribute(keyFields[i]).DatabaseColumn).Append("]");
                if (i < numKeyFields - 1)
                {
                    query.Append(", ");
                }
            }
            query.Append(" from [").Append(TableName).Append("] group by ");
            for (int i = 1; i <= numKeyFields; i++)
            {
                query.Append(i);
                if (i < numKeyFields)
                {
                    query.Append(", ");
                }
            }
            query.Append(" having count(*) > 1");
            return query.ToString();
        }

        #region Private Functions
        /// <summary>
        /// Gets a data table representing this object, based on previously specified values for the keys.
        /// </summary>
        private DataTable GetDefaultData(bool lockRecord = false)
        {
            if (IsLocked)
            {
                return DatabaseData.Table;
            }
            else if (lockRecord)
            {
                List<IDbDataParameter> parameters;
                switch (DatabaseType)
                {
                    case Database.DatabaseType.Advantage:
                        DatabaseData = Global.AdsConn.GetDataAndLock(GetDefaultDataQuery(Global.AdsConn, out parameters), parameters);
                        break;
                    case Database.DatabaseType.SqlServer:
                        var conn = new SqlServerConnection();
                        DatabaseData = conn.GetDataAndLock(GetDefaultDataQuery(conn, out parameters), parameters);
                        break;
                    default:
                        throw UnsupportedDatabaseTypeException;
                }
                return DatabaseData.Table;
            }
            else
            {
                DataTable dt;
                DatabaseConnection conn = GetConnection();
                try
                {
                    dt = conn.GetData(GetDefaultDataQuery(conn, out List<IDbDataParameter> parameters), parameters);
                }
                catch
                {
                    if (!conn.IsGlobal) conn.Dispose();
                    throw;
                }
                if (!conn.IsGlobal) conn.Dispose();
                return dt;
            }
        }

        private DataTable GetNullData()
        {
            if (IsLocked)
            {
                return DatabaseData.Table.Clone();
            }
            else
            {
                DataTable dt;
                DatabaseConnection conn = GetConnection();
                try
                {
                    dt = conn.GetData(GetNullDataQuery());
                }
                catch
                {
                    if (!conn.IsGlobal) conn.Dispose();
                    throw;
                }
                if (!conn.IsGlobal) conn.Dispose();
                return dt;
            }
        }

        /// <summary>
        /// Sets a value into a specific field.
        /// </summary>
        /// <param name="mi">The field to be set into.</param>
        /// <param name="value">The value to set.</param>
        protected void SetFieldValue(MemberInfo mi, object value)
        {
            Database.DataType dataType = GetDataType(mi);

            // Always initialize strings to "" when null.
            if ((value == DBNull.Value || value == null) && !dataType.EqualsAnyOf(Database.DataType.String, Database.DataType.Character))
            {
                mi.SetValue(this, null);
                return;
            }

            // Handle nullable fields/properties.
            Type type = mi.GetUnderlyingType();
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                if (underlyingType.IsEnum)
                {
                    mi.SetValue(this, Enum.ToObject(underlyingType, value));
                    return;
                }
                else
                {
                    value = ConvertToNull(dataType, Convert.ChangeType(value, underlyingType));
                }
            }

            string valueStringTrimmed = value.ToString().Trim();

            // Handle enums.
            if (type.IsEnum)
            {
                mi.SetValue(this, Enum.Parse(type, valueStringTrimmed));
                return;
            }

            switch (dataType)
            {
                case Database.DataType.String:
                    // Trimming off whitespace so that string fields cannot end with odd characters such as '\n', causing queries matching on an exact string to fail to return that record.
                    mi.SetValue(this, valueStringTrimmed);
                    return;
                case Database.DataType.Character:
                    // Trimming off whitespace so that string fields cannot end with odd characters such as '\n', causing queries matching on an exact string to fail to return that record.
                    if (valueStringTrimmed.Length == 0)
                        mi.SetValue(this, default(char));
                    else
                        mi.SetValue(this, char.Parse(valueStringTrimmed));
                    return;
                case Database.DataType.Boolean:
                    if (valueStringTrimmed.Length == 0)
                        mi.SetValue(this, default(bool));
                    else
                        mi.SetValue(this, bool.Parse(valueStringTrimmed));
                    return;
                case Database.DataType.Decimal:
                    if (valueStringTrimmed.Length == 0)
                        mi.SetValue(this, default(decimal));
                    else
                        mi.SetValue(this, decimal.Parse(valueStringTrimmed));
                    return;
                case Database.DataType.Integer:
                    if (valueStringTrimmed.Length == 0)
                        mi.SetValue(this, default(int));
                    else
                        mi.SetValue(this, int.Parse(valueStringTrimmed));
                    return;
                case Database.DataType.Long:
                    if (valueStringTrimmed.Length == 0)
                        mi.SetValue(this, default(long));
                    else
                        mi.SetValue(this, long.Parse(valueStringTrimmed));
                    return;
                case Database.DataType.Date:
                    if (valueStringTrimmed.Length == 0)
                        mi.SetValue(this, default(DateTime));
                    else
                        mi.SetValue(this, DateTime.Parse(valueStringTrimmed));
                    return;
                case Database.DataType.Time:
                    if (valueStringTrimmed.Length == 0)
                        mi.SetValue(this, default(TimeSpan));
                    else
                        mi.SetValue(this, TimeSpan.Parse(valueStringTrimmed));
                    return;
                default:
                    throw new NotSupportedException(dataType.ToString());
            }
        }

        private object ConvertToNull(Database.DataType dataType, object value)
        {
            if (dataType == Database.DataType.String && String.IsNullOrEmpty(value.ToString())) return null;
            //if (dataType == Database.DataType.Integer && (int)value == 0) return null;
            //if (dataType == Database.DataType.Decimal && (decimal)value == 0) return null;
            //if (dataType == Database.DataType.Date && (DateTime)value == default(DateTime)) return null;
            return value;
        }

        private DatabaseTableAttribute GetTableAttribute()
        {
            return DatabaseTableCache.TableAttributes.GetOrAdd(this.Type, DatabaseTableCache.LoadTableAttribute);
        }

        protected DatabaseAttribute GetAttribute(MemberInfo mi)
        {
            return DatabaseTableCache.ColumnAttributes.GetOrAdd(mi, DatabaseTableCache.LoadColumnAttribute);
        }

        private ReadOnlyCollection<MemberInfo> GetAllFields()
        {
            return DatabaseTableCache.FieldMemberInfos.GetOrAdd(this.Type, DatabaseTableCache.LoadAllFields);
        }

        private ReadOnlyCollection<MemberInfo> GetAllLazyMembers()
        {
            return DatabaseTableCache.LazyMemberInfos.GetOrAdd(this.Type, DatabaseTableCache.LoadAllLazyMemberInfos);
        }

        private ReadOnlyCollection<PropertyInfo> GetAllDetailProperties()
        {
            return DatabaseTableCache.DetailPropertyInfos.GetOrAdd(this.Type, DatabaseTableCache.LoadAllDetailPropertyInfos);
        }

        /// <summary>
        /// Determines what the Database.DataType for a given field is.
        /// Will use the specified value if one was set, or will try to determine it otherwise.
        /// </summary>
        protected Database.DataType GetDataType(MemberInfo mi)
        {
            return GetDataType(mi, out _);
        }
        /// <summary>
        /// Determines what the Database.DataType for a given field is.
        /// Will use the specified value if one was set, or will try to determine it otherwise.
        /// </summary>
        protected Database.DataType GetDataType(MemberInfo mi, out Type underlyingType)
        {
            Database.DataType dataType = GetAttribute(mi).Type;
            if (dataType == Database.DataType.Unknown)
            {
                underlyingType = mi.GetUnderlyingType();

                if (underlyingType.IsGenericType) underlyingType = Nullable.GetUnderlyingType(underlyingType);

                if (underlyingType == typeof(int) || underlyingType.IsEnum) return Database.DataType.Integer;
                if (underlyingType == typeof(decimal)) return Database.DataType.Decimal;
                if (underlyingType == typeof(string)) return Database.DataType.String;
                if (underlyingType == typeof(char)) return Database.DataType.Character;
                if (underlyingType == typeof(bool)) return Database.DataType.Boolean;
                if (underlyingType == typeof(long)) return Database.DataType.Long;
                if (underlyingType == typeof(DateTime)) return Database.DataType.Date;
                if (underlyingType == typeof(TimeSpan)) return Database.DataType.Time;
                return Database.DataType.Unknown;
            }
            underlyingType = null;
            return dataType;
        }

        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (this.IsDisposed)
            {
                return;
            }

            if (IsLocked)
            {
                this.DatabaseData.UnlockWithoutUpdatingData();
            }
            if (this.DatabaseData?.Connection?.IsGlobal == false)
            {
                this.DatabaseData.Connection.Dispose();
                this.DatabaseData.Connection = null;
            }
            this.IsDisposed = true;
        }
        #endregion
    }

    public static class MemberHelper
    {
        public static void SetValue(this MemberInfo mi, object target, object value)
        {
            switch (mi.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)mi).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)mi).SetValue(target, value, null);
                    break;
                default:
                    throw new NotSupportedException(mi.MemberType.ToString());
            }
        }

        public static object GetValue(this MemberInfo mi, object target)
        {
            switch (mi.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)mi).GetValue(target);
                case MemberTypes.Property:
                    return ((PropertyInfo)mi).GetValue(target, null);
                default:
                    throw new NotSupportedException(mi.MemberType.ToString());
            }
        }

        public static Type GetUnderlyingType(this MemberInfo mi)
        {
            switch (mi.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)mi).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)mi).PropertyType;
                case MemberTypes.Method:
                    return ((MethodInfo)mi).ReturnType;
                case MemberTypes.Event:
                    return ((EventInfo)mi).EventHandlerType;
                default:
                    throw new NotSupportedException(mi.MemberType.ToString());
            }
        }

        public static bool IsDuplicate<TABLE>(this TABLE first, TABLE second) where TABLE : DatabaseTable
        {
            if (first == second) return false; // An item is never a duplicate of itself.  This is how this differs from testing equality.
            return first.GetKeyMembers().All(x => x.GetValue(first).Equals(x.GetValue(second)));
        }
    }
}

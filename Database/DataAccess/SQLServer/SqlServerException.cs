using System.Data.SqlClient;

namespace ww.Tables
{
    internal class SqlServerException : DatabaseException
    {
        internal SqlServerException() : base() { }
        internal SqlServerException(SqlException ex) : base(ex.Message, ex) { }
        internal SqlServerException(string message, SqlException ex) : base(message, ex) { }

        #region Error Numbers
        internal override int ErrorNumber => this.InnerException is SqlException ex ? ex.Number : 0;

        internal enum ErrorNumberEnum
        {
            Timeout = -2,
            BadSqlStatement = 102,
            ColumnNotFound = 207,
            TableNotFound = 208,
            LockFailed = 1222,
            UniqueIndexViolation = 2601,
            KeyViolation = 2627,
            DataTruncationAvoidedExtended = 2628,
            DataTruncationAvoided = 8152,
            OrderByColumnFailed = 9999,
        }

        public override bool LockFailed => ErrorNumber == (int)ErrorNumberEnum.LockFailed;
        public override bool Timeout => ErrorNumber == (int)ErrorNumberEnum.Timeout;
        public override bool OrderByColumnFailed => ErrorNumber == (int)ErrorNumberEnum.OrderByColumnFailed;
        public override bool KeyViolation => ErrorNumber == (int)ErrorNumberEnum.KeyViolation || ErrorNumber == (int)ErrorNumberEnum.UniqueIndexViolation;
        public override bool ConnectionFailure => false;
        #endregion
    }

    internal class SqlServerConnectionException : SqlServerException
    {
        internal SqlServerConnectionException(SqlException ex) : base($"Connection failure. {ex.Message}", ex) { }
    }

    internal class SqlServerDataTruncationException : SqlServerException, IDatabaseDataTruncationException
    {
        public string ColumnName { get; private set; }
        public int? ColumnSize { get; private set; }
        public object ValueBeingTruncated { get; private set; }

        internal SqlServerDataTruncationException(string columnName, int? columnSize, object valueBeingTruncated, SqlException ex) : base(ex)
        {
            ColumnName = columnName;
            ColumnSize = columnSize;
            ValueBeingTruncated = valueBeingTruncated;
        }

        public string UserFriendlyMessage
        {
            get { return ValueBeingTruncated == null ? null : $"Cannot save because the following text is too long for the corresponding database field, which has a maximum of {ColumnSize} characters. The text below is {ValueBeingTruncated.ToString().Length} characters.\n\n{ValueBeingTruncated}"; }
        }
    }
}

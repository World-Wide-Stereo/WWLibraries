using Advantage.Data.Provider;
using ww.Utilities.Extensions;

namespace ww.Tables
{
    internal class AdvantageException : DatabaseException
    {
        internal AdvantageException() : base() { }
        internal AdvantageException(AdsException ex) : base(ex.Message, ex) { }
        internal AdvantageException(string message, AdsException ex) : base(message, ex) { }

        #region Error Numbers
        internal override int ErrorNumber
        {
            get
            {
                var ex = this.InnerException as AdsException;
                return ex == null ? 0 : ex.Number;
            }
        }

        internal enum ErrorNumberEnum
        {
            LockFailed = 5035,

            BadSqlStatement = 2115,
            ColumnNotFound = 2121,
            OrderByColumnFailed = 2145,
            AggregateFunctionNotAllowed = 2149,
            ScalarFunctionArgumentInvalid = 2159,
            NumericOverflow = 2232,
            InvalidHandle = 5018,
            InvalidTableHandle = 5023,
            DataTruncationAvoided = 5071,
            TableClosedButHeldInCache = 5094,
            UnicodeNotSupported = 5211,
            DestinationNotAvailable = 6410,
            DiscoveryFailure = 6420,
            ServerDidntRespond = 6610,
            CircuitReset = 6624,
            //FileOpenFailed = 7008,
            TableNotFound = 7041,
            UserNotConnected = 7050,
            KeyViolation = 7057,
            //GeneralSqlError = 7200,
            Timeout = 7209,
        }

        public override bool LockFailed
        {
            get { return this.ErrorNumber == (int)ErrorNumberEnum.LockFailed; }
        }
        public override bool Timeout
        {
            get { return this.ErrorNumber == (int)ErrorNumberEnum.Timeout; }
        }
        public override bool OrderByColumnFailed
        {
            get { return this.ErrorNumber == (int)ErrorNumberEnum.OrderByColumnFailed; }
        }
        public override bool KeyViolation
        {
            get { return this.ErrorNumber == (int)ErrorNumberEnum.KeyViolation; }
        }
        public override bool ConnectionFailure
        {
            get { return ((ErrorNumberEnum)this.ErrorNumber).EqualsAnyOf(ErrorNumberEnum.ServerDidntRespond, ErrorNumberEnum.DiscoveryFailure, ErrorNumberEnum.CircuitReset, ErrorNumberEnum.DestinationNotAvailable); }
        }
        #endregion
    }

    internal class AdvantageConnectionException : AdvantageException
    {
        internal AdvantageConnectionException(AdsException ex) : base("Connection failure. " + ex.Message, ex) { }
    }

    internal class AdvantageDataTruncationException : AdvantageException, IDatabaseDataTruncationException
    {
        public string ColumnName { get; private set; }
        public int? ColumnSize { get; private set; }
        public object ValueBeingTruncated { get; private set; }

        internal AdvantageDataTruncationException(string columnName, int? columnSize, object valueBeingTruncated, AdsException ex) : base(ex)
        {
            this.ColumnName = columnName;
            this.ColumnSize = columnSize;
            this.ValueBeingTruncated = valueBeingTruncated;
        }

        public string UserFriendlyMessage
        {
            get { return "Cannot save because the following text is too long for the corresponding database field, which has a maximum of " + this.ColumnSize + " characters. The text below is " + this.ValueBeingTruncated.ToString().Length + " characters.\n\n" + this.ValueBeingTruncated; }
        }
    }
}

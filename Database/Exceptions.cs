using System;

namespace ww.Tables
{
    public abstract class DatabaseException : Exception
    {
        protected DatabaseException() : base() { }
        protected DatabaseException(Exception ex) : base(ex.Message, ex) { }
        protected DatabaseException(string message, Exception ex) : base(message, ex) { }

        #region Error Numbers
        internal abstract int ErrorNumber { get; }

        public abstract bool LockFailed { get; }
        public abstract bool Timeout { get; }
        public abstract bool OrderByColumnFailed { get; }
        public abstract bool KeyViolation { get; }
        public abstract bool ConnectionFailure { get; }
        #endregion

        #region Lock Failure Handler
        public delegate bool LockFailureHandler(string message);
        public static event LockFailureHandler HandleLockFailure;

        internal static bool ReattemptOnLockFailure(string exceptionMsg, int errorNumber, int lockFailedErrorNumber)
        {
            if (HandleLockFailure != null && errorNumber == lockFailedErrorNumber)
            {
                return HandleLockFailure(exceptionMsg);
            }
            return false;
        }

        public static LockFailureHelper UsingLockFailureHandler(LockFailureHandler handler)
        {
            return new LockFailureHelper(handler);
        }

        public class LockFailureHelper : IDisposable
        {
            private LockFailureHandler Handler;

            internal LockFailureHelper(LockFailureHandler handler)
            {
                Handler = handler;
                HandleLockFailure += Handler;
            }

            public void Dispose()
            {
                HandleLockFailure -= Handler;
            }
        }
        #endregion
    }

    public interface IDatabaseDataTruncationException
    {
        string ColumnName { get; }
        int? ColumnSize { get; }
        object ValueBeingTruncated { get; }
        string UserFriendlyMessage { get; }
    }

    public class DataRequirementException : Exception
    {
        public string FailedFieldVarName;

        public DataRequirementException(string message, string failedFieldVarName) : base(message) { FailedFieldVarName = failedFieldVarName; }
    }
}

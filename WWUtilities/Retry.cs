using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ww.Utilities
{
    public static class Retry
    {
        /// <summary>
        /// Provides generic retry logic for an Action.
        /// </summary>
        /// <param name="action">Action to perform the retries on.</param>
        /// <param name="exceptionType">Type of exception to retry on. If null, retries on any type of exception.</param>
        /// <param name="skipRetryOnExceptionTypes">Type of exception to skip retries on. If null, retries on any type of exception.</param>
        /// <param name="retryInterval">Optional parameter for the time interval in MS between retries.</param>
        /// <param name="tryCount">Optional parameter for the number of times to try.</param>
        public static void Do(Action action, Type exceptionType = null, IEnumerable<Type> skipRetryOnExceptionTypes = null, int retryInterval = 1000, int tryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, exceptionType, skipRetryOnExceptionTypes, retryInterval, tryCount);
        }

        /// <summary>
        /// Provides generic retry logic for a Func.
        /// </summary>
        /// <typeparam name="T">Return type of the function being retried.</typeparam>
        /// <param name="func">Func to perform the retries on.</param>
        /// <param name="exceptionType">Type of exception to retry on. If null, retries on any type of exception.</param>
        /// <param name="skipRetryOnExceptionTypes">Type of exception to skip retries on. If null, retries on any type of exception.</param>
        /// <param name="retryInterval">Optional parameter for the time interval in MS between retries.</param>
        /// <param name="tryCount">Optional parameter for the number of times to try.</param>
        /// <returns>Result of the function specified by the func parameter.</returns>
        public static T Do<T>(Func<T> func, Type exceptionType = null, IEnumerable<Type> skipRetryOnExceptionTypes = null, int retryInterval = 1000, int tryCount = 3)
        {
            for (int retry = 0; retry < tryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        Thread.Sleep(retryInterval);
                    }
                    return func();
                }
                catch (Exception ex) when ((exceptionType == null || exceptionType == ex.GetType()) && (skipRetryOnExceptionTypes == null || !skipRetryOnExceptionTypes.Contains(ex.GetType())) && retry != tryCount - 1) { }
            }

            // This will never be hit because we will either return the result of func() or throw the last exception.
            // This must be here because the compiler doesn't recognize that this code is unreachable and expects something to be returned.
            throw new Exception();
        }
    }
}

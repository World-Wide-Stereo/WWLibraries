using System;
using System.Collections.Generic;

namespace ww.Utilities
{
    public class ToDictionaryException : ArgumentException
    {
        public IEnumerable<object> Collection;
        public object Key;
        public object Value;

        public ToDictionaryException(IEnumerable<object> collection, object key, object value, string message) : base(message)
        {
            Collection = collection;
            Key = key;
            Value = value;
        }
    }
}

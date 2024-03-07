using System;
using System.Collections.Generic;

namespace ww.Utilities
{
    public class AppendOnlyList<T> : List<T>
    {
        [Obsolete("You may not call Remove() on this type of object",true)]
        public new void Remove(T item)
        {
            throw new NotSupportedException("You may not call Remove() on this type of object.");
        }
        [Obsolete("You may not call RemoveAt() on this type of object", true)]
        public new void RemoveAt(int index)
        {
            throw new NotSupportedException("You may not call RemoveAt() on this type of object.");
        }
        [Obsolete("You may not call RemoveAll() on this type of object", true)]
        public new void RemoveAll(Predicate<T> match)
        {
            throw new NotSupportedException("You may not call RemoveAll() on this type of object.");
        }
        [Obsolete("You may not call RemoveRange() on this type of object", true)]
        public new void RemoveRange(int index, int count)
        {
            throw new NotSupportedException("You may not call RemoveRange() on this type of object.");
        }
    }
}

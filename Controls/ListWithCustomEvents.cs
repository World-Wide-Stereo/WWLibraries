using System;
using System.Collections.Generic;
using System.Linq;

namespace Controls
{
    public class ListWithCustomEvents<T> : List<T>
    {
        public delegate void ListEventHandler(object sender, ListEventArgs e);
        public event ListEventHandler OnAdd;
        public event ListEventHandler OnRemove;
        public event EventHandler OnClear;

        public new void Add(T item)
        {
            if (OnAdd != null) OnAdd(this, new ListEventArgs { Items = new List<T> { item } });
            base.Add(item);
        }
        public new void AddRange(IEnumerable<T> collection)
        {
            var items = collection.ToList();
            if (OnAdd != null) OnAdd(this, new ListEventArgs { Items = items });
            base.AddRange(items);
        }
        public void AddRange(List<T> items)
        {
            if (OnAdd != null) OnAdd(this, new ListEventArgs { Items = items });
            base.AddRange(items);
        }

        public new void Remove(T item)
        {
            if (OnRemove != null) OnRemove(this, new ListEventArgs { Items = new List<T> { item } });
            base.Remove(item);
        }
        public new void RemoveAll(Predicate<T> match)
        {
            if (OnRemove != null) OnRemove(this, new ListEventArgs { Items = this.FindAll(match) });
            base.RemoveAll(match);
        }
        public new void RemoveAt(int index)
        {
            if (OnRemove != null) OnRemove(this, new ListEventArgs { Items = new List<T> { this[index] } });
            base.RemoveAt(index);
        }
        public new void RemoveRange(int index, int count)
        {
            if (OnRemove != null) OnRemove(this, new ListEventArgs { Items = this.GetRange(index, count) });
            base.RemoveRange(index, count);
        }

        public new void Clear()
        {
            if (OnClear != null) OnClear(this, new EventArgs());
            base.Clear();
        }

        public class ListEventArgs : EventArgs
        {
            public List<T> Items;
        }
    }
}

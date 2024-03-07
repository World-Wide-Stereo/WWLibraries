using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ww.Utilities.Extensions;

namespace ww.Utilities
{
    public class ListDictionary<TKey, TValue> : Dictionary<TKey, IList<TValue>>, IListDictionary<TKey, TValue>
    {
        public ListDictionary() : base() {}

        public ListDictionary(Dictionary<TKey, IEnumerable<TValue>> dictionary)
        {
            foreach(var key in dictionary.Keys)
            {
                this.Add(key, dictionary[key].ToList());
            }
        }
        public ListDictionary(Dictionary<TKey, IList<TValue>> dictionary)
        {
            foreach (var key in dictionary.Keys)
            {
                this.Add(key, dictionary[key]);
            }
        }

        public void Add(TKey key, IEnumerable<TValue> value)
        {
            base.Add(key, value.ToList());
        }
        public void AddToList(TKey key, TValue value)
        {
            IDictionaryExtensions.AddToList(this, key, value);
        }
        public void AddToList(TKey key, IEnumerable<TValue> values)
        {
            if (this.ContainsKey(key))
            {
                foreach (var v in values)
                {
                    this[key].Add(v);
                }
            }
            else
            {
                this[key] = values.ToList();
            }
        }
    }

    public interface IListDictionary<TKey, TValue> : IDictionary<TKey, IList<TValue>>
    {
        
    }


}

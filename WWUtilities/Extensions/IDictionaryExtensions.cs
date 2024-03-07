using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class IDictionaryExtensions
    {
        public static void Update<TKEY, TVALUE>(this IDictionary<TKEY, TVALUE> dictionary, TKEY key, TVALUE value)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
            }
            else
            {
                dictionary.Add(key, value);
            }
        }

        public static void AddToList<TKEY, TVALUE>(this IDictionary<TKEY, IList<TVALUE>> dictionary, TKEY key, TVALUE value)
        {
            if (dictionary.ContainsKey(key) && dictionary[key] != null)
            {
                dictionary[key].Add(value);
            }
            else
            {
                dictionary.Update(key, new List<TVALUE> {value});
            }
        }

        public static void AddRange<TKEY, TVALUE>(this IDictionary<TKEY, TVALUE> dictionary, IEnumerable<KeyValuePair<TKEY, TVALUE>> values)
        {
            foreach (var value in values)
            {
                dictionary.Add(value);
            }
        }

        public static void RemoveAll<TKEY, TVALUE>(this IDictionary<TKEY, TVALUE> dict, Func<TKEY, TVALUE, bool> match)
        {
            foreach (var key in dict.Keys.ToArray().Where(key => match(key, dict[key])))
            {
                dict.Remove(key);
            }
        }

        public static TVALUE GetValueOrDefault<TKEY, TVALUE>(this IDictionary<TKEY, TVALUE> dictionary, TKEY key)
        {
            if (key == null) return default(TVALUE);
            TVALUE returnValue;
            return dictionary.TryGetValue(key, out returnValue) ? returnValue : default(TVALUE);
        }
        public static TVALUE GetValueOrDefault<TKEY, TVALUE>(this IDictionary<TKEY, TVALUE> dictionary, TKEY key, TVALUE defaultValue)
        {
            if (key == null) return defaultValue;
            TVALUE returnValue;
            return dictionary.TryGetValue(key, out returnValue) ? returnValue : defaultValue;
        }


        public static Dictionary<TKEY, IEnumerable<TVALUE>> ToDictionary<TKEY, TVALUE>(this IEnumerable<IGrouping<TKEY, TVALUE>> group)
        {
            return group.ToDictionary(x => x.Key, x => x.AsEnumerable());
        }
        public static ListDictionary<TKEY, TVALUE> ToListDictionary<TKEY, TVALUE>(this IDictionary<TKEY, List<TVALUE>> data)
        {
            return data.ToListDictionary(x => x.Key, x => x.Value.ToList());
        }

        /// <summary>
        /// Creates a new read-only dictionary from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        public static ReadOnlyDictionary<TKey, TValue> ToReadOnlyDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> comparer = null)
        {
            return new ReadOnlyDictionary<TKey, TValue>(source.ToDictionary(keySelector, valueSelector, comparer));
        }

        /// <summary>
        /// Creates a new <see cref="ConcurrentDictionary{TKey,TValue}"/> from an <see cref="IEnumerable{T}"/>.
        /// </summary>
        public static ConcurrentDictionary<TKey, TValue> ToConcurrentDictionary<TSource, TKey, TValue>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TValue> valueSelector, IEqualityComparer<TKey> comparer = null)
        {
            return new ConcurrentDictionary<TKey, TValue>(source.ToDictionary(keySelector, valueSelector, comparer));
        }
    }
}

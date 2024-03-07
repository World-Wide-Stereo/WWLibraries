using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class IEnumerableExtensions
    {
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            return new HashSet<T>(source, comparer);
        }

        public static string ToDatabaseList<T>(this IEnumerable<T> list, string separator = ",")
        {
            return list.Join(separator);
        }
        public static string ToDatabaseList(this IEnumerable<string> list)
        {
            return "'" + list.Join("','") + "'";
        }
        public static FormattableString ToDatabaseListInterpolated<T>(this IEnumerable<T> list, bool castAsVarChar = false)
        {
            object[] args = list.Select(x => (object)x).ToArray();
            var formatBuilder = new StringBuilder();
            for (int i = 0; i < args.Length; i++)
            {
                if (i > 0)
                {
                    formatBuilder.Append(",");
                }
                if (castAsVarChar)
                {
                    formatBuilder.Append("cast({").Append(i).Append("} as varchar)");
                }
                else
                {
                    formatBuilder.Append("{").Append(i).Append("}");
                }
            }
            return FormattableStringFactory.Create(formatBuilder.ToString(), args);
        }
        public static string Join<T>(this IEnumerable<T> list, string joiner)
        {
            var output = new StringBuilder();
            foreach (object obj in list)
            {
                if (output.Length > 0)
                {
                    output.Append(joiner);
                }
                if (obj != null)
                {
                    output.Append(obj);
                }
            }
            return output.ToString();
        }
        public static string JoinConjunction<T>(this IEnumerable<T> data, string joiner = ", ", string conjunction = "and", bool finalSeparator = false)
        {
            List<T> list = data.ToList();
            int count = list.Count;
            var output = new StringBuilder();
            foreach (object obj in list.Take(count - 1))
            {
                if (output.Length > 0)
                {
                    output.Append(joiner);
                }
                if (obj != null)
                {
                    output.Append(obj);
                }
            }
            if (count >= 2 && finalSeparator)
            {
                output.Append(joiner.Trim());
            }
            if (count > 1)
            {
                output.Append(" " + conjunction + " ");
            }
            output.Append(list.Last());
            return output.ToString();
        }
        public static IEnumerable<T> Disjunction<T>(this IEnumerable<T> setA, IEnumerable<T> setB)
        {
            HashSet<T> data = new HashSet<T>(setA);
            data.SymmetricExceptWith(setB);
            return data;
        }
        public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T toRemove)
        {
            return list.Except(new[] { toRemove });
        }

        /// <summary>
        /// Joins a dictionary as a parameter string
        /// </summary>
        /// <param name="list"></param>
        /// <param name="pairSeparator">Separates each key/value pair</param>
        /// <param name="pairAttacher">Separates the key from the value within a pair</param>
        /// <returns></returns>
        public static string JoinParameters(this IEnumerable<KeyValuePair<string, string>> list, string pairSeparator = "&", string pairAttacher = "=")
        {
            StringBuilder output = new StringBuilder("");
            foreach (KeyValuePair<string, string> o in list)
            {
                if (output.Length > 0)
                    output.Append(pairSeparator);
                output.Append(o.Key);
                output.Append(pairAttacher);
                output.Append(o.Value);
            }
            return output.ToString();
        }

        public static IEnumerable<T> Distinct<T, TRet>(this IEnumerable<T> list, Func<T, TRet> comparison, Func<T, TRet> sort = null, bool sortDesc = false)
        {
            if (sort == null)

                return list.GroupBy(comparison).Select(x => x.First());
            else if (sortDesc)
                return list.GroupBy(comparison).Select(x => x.OrderByDescending(sort).First());
            else
                return list.GroupBy(comparison).Select(x => x.OrderBy(sort).First());

        }

        public static int WeightedAverage<T>(this IEnumerable<T> records, Func<T, int> value, Func<T, int> weight)
        {
            int weightedValueSum = records.Sum(record => value(record) * weight(record));
            int weightSum = records.Sum(record => weight(record));

            if (weightSum == 0) return 0;
            return weightedValueSum / weightSum;
        }
        public static decimal WeightedAverage<T>(this IEnumerable<T> records, Func<T, decimal> value, Func<T, decimal> weight)
        {
            decimal weightedValueSum = records.Sum(record => value(record) * weight(record));
            decimal weightSum = records.Sum(record => weight(record));

            if (weightSum == 0) return 0;
            return weightedValueSum / weightSum;
        }

        public static bool ContainsAny<T>(this IEnumerable<T> list, IEnumerable<T> values)
        {
            return values.Any(x => list.Contains(x));
        }
        public static bool ContainsAny<T>(this IEnumerable<T> list, params T[] values)
        {
            return values.Any(x => list.Contains(x));
        }

        public static bool ContainsAll<T>(this IEnumerable<T> list, IEnumerable<T> values)
        {
            return values.All(x => list.Contains(x));
        }
        public static bool ContainsAll<T>(this IEnumerable<T> list, params T[] values)
        {
            return values.All(x => list.Contains(x));
        }

        #region Get random value https://stackoverflow.com/questions/2019417/
        public static T PickRandom<T>(this IEnumerable<T> source)
        {
            return source.PickRandom(1).Single();
        }
        public static IEnumerable<T> PickRandom<T>(this IEnumerable<T> source, int count)
        {
            return source.Shuffle().Take(count);
        }
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy(x => Guid.NewGuid());
        }
        #endregion

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> list, T value)
        {
            return list.Concat(new[] { value });
        }

        public static ListDictionary<TKey, TElement> ToListDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, IEnumerable<TElement>> elementSelector)
        {
            var dictionary = source.ToDictionary(keySelector, elementSelector);
            return new ListDictionary<TKey, TElement>(dictionary);
        }

        public static ListDictionary<TKEY, TVALUE> ToListDictionary<TKEY, TVALUE>(this IEnumerable<IGrouping<TKEY, TVALUE>> data)
        {
            return data.ToListDictionary(x => x.Key, x => x.Select(y => y));
        }

        /// <summary>
        /// Break a list of items into chunks of a specific size
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunksize)
        {
            while (source.Any())
            {
                yield return source.Take(chunksize);
                source = source.Skip(chunksize);
            }
        }

        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        private static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
        ///<summary>Finds the index of the first occurence of an item in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="item">The item to find.</param>
        ///<returns>The index of the first matching item, or -1 if the item was not found.</returns>
        public static int IndexOf<T>(this IEnumerable<T> items, T item)
        {
            return items.FindIndex(i => EqualityComparer<T>.Default.Equals(item, i));
        }

        public static bool AreIntegersConsecutive(this IEnumerable<int> listOfIntegers)
        {
            // Example 1 - Integers are Consecutive
            // Input is { 5, 6, 7, 8 }
            // Select yields { (5-0=)5, (6-1=)5, (7-2=)5, (8-3=)5 }
            // Distinct yields { 5, (5 not distinct, 5 not distinct, 5 not distinct) }
            // Skip yields { (5 skipped, nothing left) }
            // Any returns false

            // Example 2 - Integers are NOT Consecutive
            // Input is { 1, 2, 6, 7 }
            // Select yields { (1-0=)1, (2-1=)1, (6-2=)4, (7-3=)4 } *
            // Distinct yields { 1, (1 not distinct,) 4, (4 not distinct) } *
            // Skip yields { (1 skipped,) 4 }
            // Any returns true

            return !listOfIntegers.Select((i, j) => i - j).Distinct().Skip(1).Any();
        }

        /// <summary>ForEach extension method for IEnumerable.</summary>
        /// <typeparam name="T">Type of the collection.</typeparam>
        /// <param name="lst">Collection being operated on.</param>
        /// <param name="act">Action to apply to each member of the collection.</param>
        public static void ForEach<T>(this IEnumerable<T> lst, Action<T> act)
        {
            foreach (var obj in lst)
            {
                act(obj);
            }
        }

        /// <summary>ForEach extension method that allows breaking out of the loop.</summary>
        /// <typeparam name="T">Type of object in the collection being operated on.</typeparam>
        /// <param name="lst">Collection being operated on.</param>
        /// <param name="fnc">
        ///     Function returning a bool that is applied to each member of the collection.
        ///     If the function returns false, then the ForEach loop is broken out of.
        /// </param>
        public static void ForEachBreakable<T>(this IEnumerable<T> lst, Func<T, bool> fnc)
        {
            foreach (var obj in lst)
            {
                if (!fnc(obj))
                {
                    break;
                }
            }
        }

        /// <summary>ForEach extension method that offers more straight-forward syntax for calling Parallel.ForEach().</summary>
        /// <typeparam name="T">Type of the collection.</typeparam>
        /// <param name="lst">Collection being operated on.</param>
        /// <param name="act">Action to apply to each member of the collection.</param>
        public static void ForEachParallel<T>(this IEnumerable<T> lst, Action<T> act)
        {
            Parallel.ForEach(lst, act);
        }

        /// <summary>
        /// ForEach extension method that completes each action in Parallel.ForEach(), throwing all exceptions at the end within an AggregateException.
        /// The normal Parallel.ForEach() will throw the first exception encountered, halting everything.
        /// From: http://msdn.microsoft.com/en-us/library/dd460695%28v=vs.100%29.aspx
        /// </summary>
        /// <typeparam name="T">Type of the collection.</typeparam>
        /// <param name="lst">Collection being operated on.</param>
        /// <param name="act">Action to apply to each member of the collection.</param>
        public static void ForEachParallelSafe<T>(this IEnumerable<T> lst, Action<T> act)
        {
            var exceptions = new ConcurrentQueue<Exception>();
            // Execute the complete loop and capture all exceptions.
            Parallel.ForEach(lst, t =>
            {
                try
                {
                    act(t);
                }
                // Store the exception and continue with the loop.
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });

            // Throw the exceptions here after the loop completes.
            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Calculates a standard deviation of elements, using a specified selector.
        /// </summary>
        public static double? GetStandardDeviation<T>(this IEnumerable<T> enumerable, Func<T, decimal> selector)
        {
            return GetStandardDeviation(enumerable, selector, out _);
        }
        /// <summary>
        /// Calculates a standard deviation of elements, using a specified selector.
        /// </summary>
        public static double? GetStandardDeviation<T>(this IEnumerable<T> enumerable, Func<T, decimal> selector, out decimal? average)
        {
            var list = enumerable as List<T> ?? enumerable.ToList();
            if (list.Count == 0)
            {
                average = null;
                return null;
            }

            decimal sum = 0;
            average = list.Average(selector);
            foreach (T item in list)
            {
                decimal diff = selector(item) - (decimal)average;
                sum += diff * diff;
            }
            return Math.Sqrt((double)sum / list.Count);
        }
        /// <summary>
        /// Filters elements to remove outliers. The enumeration will be selected three times, first to calculate an average, second
        /// for a standard deviation, and third to yield remiaining elements. The outliers are these elements which are further
        /// from an average than k*(standard deviation). Set k=3 for standard three-sigma rule.
        /// </summary>
        public static IEnumerable<T> RemoveOutliers<T>(this IEnumerable<T> enumerable, Func<T, decimal> selector, decimal k)
        {
            // Usage Examples
            //IEnumerable<decimal> results = new[] { 1, 1.1m, 1.2m, 0.9m, 2, 0.8m };
            //decimal[] filtered;
            //// contains all elements
            //filtered = results.RemoveOutliersUsingStandardDeviation(selector: result => result, k: 3).ToArray();
            //// contains all elements except 2.0. That is, filtered={ 1, 1.1, 1.2, 0.9, 0.8 }
            //filtered = results.RemoveOutliersUsingStandardDeviation(selector: result => result, k: 2).ToArray();
            //// contains just one element, 1.2, which is closest to an average. That is, filtered={ 1.2 }
            //filtered = results.RemoveOutliersUsingStandardDeviation(selector: result => result, k: 0.1m).ToArray();
            //// a singleton is always equal to it's average, so it's yielded even with k==0.
            //// That is, filtered={ 1.2 }
            //filtered = filtered.RemoveOutliersUsingStandardDeviation(k: 0, selector: result => result).ToArray();

            var list = enumerable as List<T> ?? enumerable.ToList();
            if (list.Count == 0)
            {
                return list;
            }

            // average and standardDeviation will never be null as long as list.Any() is true.
            decimal? average;
            double? standardDeviation = list.GetStandardDeviation(selector, out average);

            decimal delta = k * (decimal)standardDeviation;
            return list.Where(item => Math.Abs(selector(item) - (decimal)average) <= delta);
        }

        public static bool IsOutlier<T>(this IEnumerable<T> enumerable, T value, Func<T, decimal> selector, decimal k)
        {
            var outliersRemoved = enumerable.ToList();
            outliersRemoved.Add(value);
            outliersRemoved.RemoveOutliers(selector, k);
            return !outliersRemoved.Contains(value);
        }

        public static Dictionary<K, V> ToDictionaryWithKeyException<TSource, K, V>(this IEnumerable<TSource> lstSource, Func<TSource, K> fncKeySelector, Func<TSource, V> fncValueSelector)
        {
            var dctReturnValue = new Dictionary<K, V>();

            foreach (var item in lstSource)
            {
                K key = fncKeySelector(item);
                V value = fncValueSelector(item);

                if (dctReturnValue.ContainsKey(key))
                {
                    throw new ToDictionaryException(lstSource.Cast<object>(), key, value, "Key " + key + " already exists in the dictionary with key type " + typeof(K) + " and value type " + typeof(V) + ".");
                }

                dctReturnValue.Add(key, value);
            }

            return dctReturnValue;
        }

        public static bool HasMinimumCount(this IEnumerable<object> enumerable, int count)
        {
            return enumerable.Skip(count - 1).Any();
        }

        #region MoreLINQ
        // From https://github.com/morelinq/MoreLINQ

        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness" is determined via a projection and the default equality
        /// comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although a set of already-seen keys is retained. If a key is seen
        /// multiple times, only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence, comparing them by the specified key projection.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy(keySelector, null);
        }
        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness" is determined via a projection and the specified comparer
        /// for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although a set of already-seen keys is retained. If a key is seen
        /// multiple times, only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <param name="comparer">The equality comparer to use to determine whether or not keys are equal. If null, the default equality comparer for <c>TSource</c> is used.</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence, comparing them by the specified key projection.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (keySelector == null) throw new ArgumentNullException("keySelector");
            return DistinctByImpl(source, keySelector, comparer);
        }
        private static IEnumerable<TSource> DistinctByImpl<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            #if !NO_HASHSET
            var knownKeys = new HashSet<TKey>(comparer);
            foreach (var element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
            #else
                // On platforms where LINQ is available but no HashSet<T>
                // (like on Silverlight), implement this operator using 
                // existing LINQ operators. Using GroupBy is slightly less
                // efficient since it has do all the grouping work before
                // it can start to yield any one element from the source.
                return source.GroupBy(keySelector, comparer).Select(g => g.First());
            #endif
        }

        /// <summary>
        /// Returns the minimal element of the given sequence, based on the given projection.
        /// </summary>
        /// <remarks>
        /// If more than one element has the minimal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current minimal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <returns>The minimal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, null);
        }
        /// <summary>
        /// Returns the minimal element of the given sequence, based on the given projection and the specified comparer for projected values.
        /// </summary>
        /// <remarks>
        /// If more than one element has the minimal projected value, the first
        /// one encountered will be returned. This operator uses immediate execution, but
        /// only buffers a single result (the current minimal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <param name="comparer">Comparer to use to compare projected values</param>
        /// <returns>The minimal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
        /// or <paramref name="comparer"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>
        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            comparer = comparer ?? Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var min = sourceIterator.Current;
                var minKey = selector(min);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, minKey) < 0)
                    {
                        min = candidate;
                        minKey = candidateProjected;
                    }
                }
                return min;
            }
        }

        /// <summary>
        /// Returns the maximal element of the given sequence, based on the given projection.
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This overload uses the default comparer
        /// for the projected type. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            return source.MaxBy(selector, null);
        }
        /// <summary>
        /// Returns the maximal element of the given sequence, based on the given projection and the specified comparer for projected values. 
        /// </summary>
        /// <remarks>
        /// If more than one element has the maximal projected value, the first
        /// one encountered will be returned. This operator uses immediate execution, but
        /// only buffers a single result (the current maximal element).
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="selector">Selector to use to pick the results to compare</param>
        /// <param name="comparer">Comparer to use to compare projected values</param>
        /// <returns>The maximal element, according to the projection.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/>, <paramref name="selector"/> 
        /// or <paramref name="comparer"/> is null</exception>
        /// <exception cref="InvalidOperationException"><paramref name="source"/> is empty</exception>
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            comparer = comparer ?? Comparer<TKey>.Default;

            using (var sourceIterator = source.GetEnumerator())
            {
                if (!sourceIterator.MoveNext())
                {
                    throw new InvalidOperationException("Sequence contains no elements");
                }
                var max = sourceIterator.Current;
                var maxKey = selector(max);
                while (sourceIterator.MoveNext())
                {
                    var candidate = sourceIterator.Current;
                    var candidateProjected = selector(candidate);
                    if (comparer.Compare(candidateProjected, maxKey) > 0)
                    {
                        max = candidate;
                        maxKey = candidateProjected;
                    }
                }
                return max;
            }
        }

        /// <summary>
        /// Returns the set of elements in the first sequence which aren't in the second sequence, according to a given key selector.
        /// </summary>
        /// <remarks>
        /// This is a set operation; if multiple elements in <paramref name="first"/> have
        /// equal keys, only the first such element is returned.
        /// This operator uses deferred execution and streams the results, although
        /// a set of keys from <paramref name="second"/> is immediately selected and retained.
        /// </remarks>
        /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="first">The sequence of potentially included elements.</param>
        /// <param name="second">The sequence of elements whose keys may prevent elements in <paramref name="first"/> from being returned.</param>
        /// <param name="keySelector">The mapping from source element to key.</param>
        /// <param name="keyComparer">The equality comparer to use to determine whether or not keys are equal. If null, the default equality comparer for <c>TSource</c> is used.</param>
        /// <returns>A sequence of elements from <paramref name="first"/> whose key was not also a key for any element in <paramref name="second"/>.</returns>
        public static IEnumerable<TSource> ExceptBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            var keys = new HashSet<TKey>(second.Select(keySelector), keyComparer);
            foreach (var element in first)
            {
                var key = keySelector(element);
                if (keys.Contains(key))
                {
                    continue;
                }
                yield return element;
                keys.Add(key);
            }
        }

        /// <summary>
        /// Returns the set of elements in the first sequence which aren't in the second sequence, according to a given key selector.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the input sequences.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="first">The sequence of potentially included elements.</param>
        /// <param name="second">The sequence of elements whose keys may prevent elements in <paramref name="first"/> from being returned.</param>
        /// <param name="keySelector">The mapping from source element to key.</param>
        /// <param name="keyComparer">The equality comparer to use to determine whether or not keys are equal. If null, the default equality comparer for <c>TSource</c> is used.</param>
        /// <returns>A sequence of elements from <paramref name="first"/> whose key was not also a key for any element in <paramref name="second"/>.</returns>
        public static IEnumerable<TSource> IntersectBy<TSource, TKey>(this IEnumerable<TSource> first, IEnumerable<TSource> second, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> keyComparer = null)
        {
            if (first == null) throw new ArgumentNullException("first");
            if (second == null) throw new ArgumentNullException("second");
            if (keySelector == null) throw new ArgumentNullException("keySelector");

            var keys = new HashSet<TKey>(first.Select(keySelector), keyComparer);
            foreach (var element in second)
            {
                var key = keySelector(element);
                // Remove the key so we only yield once.
                if (keys.Remove(key))
                {
                    yield return element;
                }
            }
        }
        #endregion
    }

    [DebuggerStepThrough]
    public static class ListExtensions
    {
        public static List<T> MoveItemToFront<T>(this List<T> list, int index)
        {
            if (index > -1 && index < list.Count)
            {
                var item = list[index];
                list.RemoveAt(index);
                list.Insert(0, item);
            }

            return list;
        }

        public static List<T> MoveItemToBack<T>(this List<T> list, int index)
        {
            if (index > -1 && index < list.Count)
            {
                var item = list[index];
                list.RemoveAt(index);
                list.Add(item);
            }

            return list;
        }
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            var item = list[oldIndex];
            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex) newIndex--;
            list.Insert(newIndex, item);
        }

        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);
                    if (newIndex > oldIndex) newIndex--;
                    list.Insert(newIndex, item);
                }
            }
        }
    }
}


namespace CoinJar.Utilities
{
    using System;
    using System.Collections.Generic;
    public static class ExtensionMethods
    {
        /// <summary>
        /// To Get distinct records
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector"></param>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// It converts enum to number
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public static int EnumToNumber(this StausCodes code)
        {
            return Convert.ToInt32(code);
        }
    }
}

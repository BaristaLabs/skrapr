namespace BaristaLabs.Skrapr.Extensions
{
    using BaristaLabs.Skrapr.Utilities;
    using System.Collections.Generic;

    public static class IListExtensions
    {
        /// <summary>
        /// Shuffles the items in the list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = RandomUtils.Random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

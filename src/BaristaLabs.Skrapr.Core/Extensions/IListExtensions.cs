namespace BaristaLabs.Skrapr.Extensions
{
    using System.Collections.Generic;
    using Troschuetz.Random;

    public static class IListExtensions
    {
        private static TRandom s_random = TRandom.New(new Troschuetz.Random.Generators.NR3Generator());

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
                int k = s_random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}

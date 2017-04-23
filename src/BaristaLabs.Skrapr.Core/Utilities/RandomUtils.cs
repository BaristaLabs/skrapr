namespace BaristaLabs.Skrapr.Utilities
{
    using Troschuetz.Random;

    /// <summary>
    /// Contains random number utilities
    /// </summary>
    public static class RandomUtils
    {
        private static TRandom s_random = TRandom.New(new Troschuetz.Random.Generators.NR3Generator());

        public static TRandom Random
        {
            get { return s_random; }
        }
    }
}

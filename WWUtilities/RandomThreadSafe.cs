using System;

namespace ww.Utilities
{
    // Adapted from https://stackoverflow.com/a/11109361
    public static class RandomThreadSafe
    {
        private static readonly Random Global = new Random();
        [ThreadStatic]
        private static Random Local;

        private static void Setup()
        {
            if (Local == null)
            {
                int seed;
                lock (Global)
                {
                    seed = Global.Next();
                }
                Local = new Random(seed);
            }
        }

        public static int Next()
        {
            Setup();
            return Local.Next();
        }
        public static int Next(int maxValue)
        {
            Setup();
            return Local.Next(maxValue);
        }
        public static int Next(int minValue, int maxValue)
        {
            Setup();
            return Local.Next(minValue, maxValue);
        }

        public static double NextDouble()
        {
            Setup();
            return Local.NextDouble();
        }

        public static void NextBytes(byte[] buffer)
        {
            Setup();
            Local.NextBytes(buffer);
        }
    }
}

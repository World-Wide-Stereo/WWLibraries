using System;
using System.Diagnostics;

namespace ww.Utilities.Extensions
{
    [DebuggerStepThrough]
    public static class NumberExtensions
    {
        public static int RoundUp(this int i, int interval)
        {
            if (interval == 0) return i;
            return ((int)Math.Ceiling((decimal)i / (decimal)interval)) * interval; 
        }
        public static int RoundDown(this int i, int interval)
        {
            if (interval == 0) return i;
            return ((int)Math.Floor((decimal)i / (decimal)interval)) * interval;
        }

        public static int RoundUp(this decimal i)
        {
            return (int)Math.Ceiling(i);
        }
        public static int RoundDown(this decimal i)
        {
            return (int)Math.Floor(i);
        }
        public static decimal RoundUp(this decimal i, int places)
        {
            var power = (decimal)Math.Pow(10, places);
            return Math.Ceiling(i * power) / power;
        }
        public static decimal RoundDown(this decimal i, int places)
        {
            var power = (decimal)Math.Pow(10, places);
            return Math.Truncate(i * power) / power;
        }
        public static decimal RoundMoney(this decimal i)
        {
            return Math.Round(i, 2, MidpointRounding.AwayFromZero);
        }


        public static decimal Sqrt(this decimal x, decimal? guess = null)
        {
            var ourGuess = guess.GetValueOrDefault(x / 2m);
            var result = x / ourGuess;
            var average = (ourGuess + result) / 2m;

            // This checks for the maximum precision possible with a decimal.
            return average == ourGuess ? average : Sqrt(x, average);
        }

        public static double ToRadian(this double val)
        {
            return val * (Math.PI / 180);
        }

        // From https://stackoverflow.com/questions/20156/
        public static string ToOrdinal(this int num)
        {
            if (num <= 0) return num.ToString();

            switch (num % 100)
            {
                case 11:
                case 12:
                case 13:
                    return num + "th";
            }

            switch (num % 10)
            {
                case 1:
                    return num + "st";
                case 2:
                    return num + "nd";
                case 3:
                    return num + "rd";
                default:
                    return num + "th";
            }
        }

        public static string ToCurrencyString(this decimal input)
        {
            return input.ToString("C");
        }

        public static string ToNumberString(this decimal input)
        {
            return input.ToString("N");
        }
    }
}

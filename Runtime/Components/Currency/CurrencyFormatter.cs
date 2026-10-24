using System;
using System.Globalization;

namespace Larje.Core.UI
{
    public static class CurrencyFormatter
    {
        public static string Format(long amount)
        {
            if (amount >= 1_000_000_000L)
            {
                return FormatTier(amount, 1_000_000_000L, "B");
            }

            if (amount >= 1_000_000L)
            {
                return FormatTier(amount, 1_000_000L, "M");
            }

            if (amount >= 1_000L)
            {
                return FormatTier(amount, 1_000L, "K");
            }

            return amount.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatTier(long amount, long divisor, string suffix)
        {
            double rounded = Math.Floor((double)amount / divisor * 10d) / 10d;
            string number = rounded % 1d == 0d
                ? ((long)rounded).ToString(CultureInfo.InvariantCulture)
                : rounded.ToString("F1", CultureInfo.InvariantCulture);
            return number + suffix;
        }
    }
}

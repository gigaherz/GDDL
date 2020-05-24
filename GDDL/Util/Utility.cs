using System;
using System.Collections.Generic;
using System.Text;

namespace GDDL.Util
{
    public static class Utility
    {
        // Ooooh... I just got how this works! Clever!
        // It's causing all the bits to spread downward
        // until all the bits below the most-significant 1
        // are also 1, then adds 1 to fill the power of two.
        public static int UpperPower(int x)
        {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        public static int CompareOrdinalIgnoreCase(this string a, string b)
        {
            return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
        }

        public static T RequireNotNull<T>(this T obj)
            where T: class
        {
            if (obj is null)
                throw new NullReferenceException();
            return (T)obj;
        }
    }
}

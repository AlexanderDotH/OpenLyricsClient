using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Utils
{
    class MathUtils
    {
        public static bool IsDoubleInRange(double  min, double max, double current)
        {
            return current > min && current < max;
        }

        public static bool IsInRange(long min, long max, long current)
        {
            return current >= min && current <= max;
        }
    }
}

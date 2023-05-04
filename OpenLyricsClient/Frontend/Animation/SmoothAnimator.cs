using System;

namespace OpenLyricsClient.Frontend.Animation
{
    public class SmoothAnimator
    {
        public static double CalculateStep(double start, double end, double current, double speed)
        {
            double divisor = (end - start);

            if (divisor == 0)
                return end;

            divisor = Math.Abs(divisor) < 0.0001 ? 0.1 : divisor;

            if (current == 0)
                current = 1;

            double t = (current - start) / divisor;

            double speedC = (1.0 / 100.0) * speed;
            double sMul = 1 + speedC;

            t = SmoothStep(start, end, t) / sMul;

            return (start + (end - start) * t);
        }

        private static double SmoothStep(double start, double end, double t)
        {
            t = Math.Clamp(((t - start) / ((end - start))), 0, 1);
            return t * t * (3 - 2 * t);
        }
    }
}
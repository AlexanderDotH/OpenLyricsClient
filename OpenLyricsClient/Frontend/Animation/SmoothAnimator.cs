using System;
using OpenLyricsClient.Frontend.Structure.Enum;

namespace OpenLyricsClient.Frontend.Animation
{
    public class SmoothAnimator
    {
        public static double Lerp(double a, double b, int tMilliseconds, double speed, EnumAnimationStyle animationStyle)
        {
            double min = Math.Min(a, b);
            double max = Math.Max(a, b);
            
            double range = max - min;
            
            double elapsedTime = (tMilliseconds / 1000.0) * speed;
            double t = elapsedTime % range;

            t = Math.Clamp(t, 0, 1);

            double progress = 0;
            
            switch (animationStyle)
            {
                case EnumAnimationStyle.SIGMOID:
                {
                    progress = Sigmoid(t, range);
                    break;
                }
                case EnumAnimationStyle.CIRCULAREASEOUT:
                {
                    progress = CircularEaseOut(t, range);
                    break;
                }
            }
            
            return min + progress * (max - min);
        }
        
        private static double Sigmoid(double t, double range)
        {
            double x = (t * 6.0) / range - 3.0;
            return 1.0 / (1.0 + Math.Exp(-x));
        }
        
        private static double CircularEaseOut(double t, double range) {
            double x = t / range;
            return Math.Sqrt(1.0 - Math.Pow(x - 1.0, 2.0));
        }
    }
}
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
            double t = elapsedTime % 1; // t should be in [0,1] range, independent from 'range' value

            t = Math.Clamp(t, 0, 1);

            double progress = 0;
        
            switch (animationStyle)
            {
                case EnumAnimationStyle.SIGMOID:
                {
                    progress = Sigmoid(t);
                    break;
                }
                case EnumAnimationStyle.CIRCULAREASEOUT:
                {
                    progress = CircularEaseOut(t);
                    break;
                }
            }
        
            return min + progress * range;
        }
    
        private static double Sigmoid(double t)
        {
            double x = t * 6.0 - 3.0;
            return 1.0 / (1.0 + Math.Exp(-x));
        }
    
        private static double CircularEaseOut(double t) 
        {
            return Math.Sqrt(1.0 - Math.Pow(t - 1.0, 2.0));
        }
    }

}
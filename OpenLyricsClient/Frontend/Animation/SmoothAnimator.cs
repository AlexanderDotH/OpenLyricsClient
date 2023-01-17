using System;
using System.Diagnostics;
using System.Threading;

namespace OpenLyricsClient.Frontend.Animation;

public class SmoothAnimator
{
    public static float CalculateStep(float start, float end, float current, float speed)
    {
        float divisor = (end - start);

        if (divisor == 0)
            return end;
        
        if (divisor == 0)
            divisor = 0.1F;

        if (current == 0)
            current = 1;
        
        float t = (current - start) / divisor;

        float speedC = (1.0F / 100.0F) * speed;
        float sMul = 1 + speedC;
        
        t = SmoothStep(start, end, t) / sMul;
        
        return (start + (end - start) * t);
    }

    private static float SmoothStep(float start, float end, float t)
    {
        t = Math.Clamp(((t - start) / ((end - start))), 0, 1);
        return t * t * (3 - 2 * t);
    }
}
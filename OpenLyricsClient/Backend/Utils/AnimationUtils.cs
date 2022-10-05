using System.Diagnostics;
using System.Threading;

namespace OpenLyricsClient.Backend.Utils;

public class AnimationUtils
{
    private double _start;
    private double _end;

    private double _duration;
    private double _time;
    private double _value;

    public AnimationUtils(double start, double end, double duration)
    {
        _start = start;
        _end = end;
        _duration = duration;
        _value = 0;

    }

    public void Start()
    {

    }

    public void Reset()
    {

    }

    private double Lerp(double p1, double p2, double t)
    {
        return  p1 + (p2 - p1) * t;
    }
    
    private double InQuint(double t) => t * t * t * t * t;
    private double InOutQuint(double t)
    {
        if (t < 0.5) return InQuint(t * 2) / 2;
        return 1 - InQuint((1 - t) * 2) / 2;
    }

    public void Update()
    {
        double t = _time / _duration;
        t = t * t * (3f - 2f * t);
        this._time += 1;
            
        _value = Lerp(this._start, this._end, InOutQuint(t));
    }
    
    public double GetValue
    {
        get
        {

            return _value;
        }
    }
}
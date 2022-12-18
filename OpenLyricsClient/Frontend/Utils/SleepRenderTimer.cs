using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Rendering;

namespace OpenLyricsClient.Frontend.Utils;

public class SleepRenderTimer : IRenderTimer
{
    private Action<TimeSpan>? _tick;
    private int _count;
    private readonly object _lock = new object();
    private bool _running;
    private readonly Stopwatch _st = Stopwatch.StartNew();
    private readonly TimeSpan _timeBetweenTicks;

    public SleepRenderTimer(int fps)
    {
        _timeBetweenTicks = TimeSpan.FromSeconds(1d / fps);
    }
        
    public event Action<TimeSpan> Tick
    {
        add
        {
            lock (_lock)
            {
                _tick += value;
                _count++;
                if (_running)
                    return;
                _running = true;
                Task.Factory.StartNew(LoopProc);
                //new Thread(LoopProc) { IsBackground = true }.Start();
            }

        }
        remove
        {
            lock (_lock)
            {
                _tick -= value;
                _count--;
            }
        }
    }

    public bool RunsInBackground => true;

    private async Task LoopProc()
    {
        var lastTick = _st.Elapsed;
        while (true)
        {
            var now = _st.Elapsed;
            var timeTillNextTick = lastTick + _timeBetweenTicks - now;
            if (timeTillNextTick.TotalMilliseconds > 1) await Task.Delay(timeTillNextTick);
            lastTick = now = _st.Elapsed;
            lock (_lock)
            {
                if (_count == 0)
                {
                    _running = false;
                    return;
                }
            }

            _tick?.Invoke(now);
                
        }
    }


}
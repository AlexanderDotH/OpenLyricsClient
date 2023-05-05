using System;
using System.Threading;
using System.Threading.Tasks;

namespace OpenLyricsClient.Shared.Utils;

public class Debouncer
{
    public static Func<T, Task<R>> Debounce<T, R>(Func<T, Task<R>> func, int milliseconds)
    {
        int last = 0;
        return async arg =>
        {
            int current = Interlocked.Increment(ref last);
            await Task.Delay(milliseconds);
            if (current == last)
                return await func(arg);
            return default(R);
        };
    }

}
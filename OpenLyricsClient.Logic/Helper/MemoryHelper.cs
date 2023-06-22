namespace OpenLyricsClient.Logic.Helper;

public class MemoryHelper
{
    public static void ForceGC()
    {
        GC.Collect(2, GCCollectionMode.Aggressive, true, true);
        GC.WaitForPendingFinalizers();
    }
}
using DevBase.Utilities;
using Microsoft.VisualBasic;

namespace OpenLyricsClient.Backend.Utils;

public class LyricsUtils
{
    public static long CalculateID(long time, string part)
    {
        string idAsString = string.Empty;
        idAsString += MemoryUtils.GetSize(part).ToString();
        return long.Parse(idAsString) + time;
    }
}
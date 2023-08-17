using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Logic.Utils;

public class TypeClassifier
{
    public static EnumElementType ClassifyLyricPart(LyricPart part)
    {
        String filtered = part.Part.Trim().Replace(" ", string.Empty);
        
        int counted = 0;

        for (int i = 0; i < filtered.Length; i++)
            if (filtered.Equals("♪"))
                counted++;

        if (counted == filtered.Length)
            return EnumElementType.NOTE;

        return EnumElementType.TEXT;
    }
}
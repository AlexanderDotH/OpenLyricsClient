using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Media;
using DevBase.Generics;
using OpenLyricsClient.Frontend.Structure;
using OpenLyricsClient.Shared.Structure.Visual;

namespace OpenLyricsClient.Frontend.Utils;

public class StringUtils
{
    public static AList<LyricOverlayElement> SplitTextToLines(string text, double width, double height, Typeface typeface, TextAlignment alignment, double fontSize)
    {
        AList<LyricOverlayElement> lines = new AList<LyricOverlayElement>();
        
        StringBuilder currentLine = new StringBuilder();
        string[] words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];

            StringBuilder candidateLine = new StringBuilder(currentLine.ToString());

            if (candidateLine.Length > 0)
                candidateLine.Append(' ');
            
            candidateLine.Append(word);

            Rect size = MeasureSingleString(candidateLine.ToString(), width, height, typeface, alignment, fontSize);
            
            if (size.Width < width)
            {
                currentLine = candidateLine;
            }
            else
            {
                lines.Add(new LyricOverlayElement(currentLine.ToString(), size));
                
                currentLine.Clear();
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(new LyricOverlayElement(
                currentLine.ToString(),
                MeasureSingleString(currentLine.ToString(), width, height, typeface, alignment, fontSize)));
        }

        return lines;
    }
    
    public static Rect MeasureSingleString(string line, double width, double height, Typeface typeface, TextAlignment alignment, double fontSize)
    {
        FormattedText formattedCandidateLine = new FormattedText(
            line, 
            typeface, 
            fontSize, 
            alignment, 
            TextWrapping.NoWrap, 
            new Size(width, height));

        return formattedCandidateLine.Bounds;
    }
}
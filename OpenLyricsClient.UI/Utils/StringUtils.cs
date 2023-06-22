using System;
using System.Text;
using Avalonia;
using Avalonia.Media;
using DevBase.Generics;
using OpenLyricsClient.UI.Structure;

namespace OpenLyricsClient.UI.Utils;

public class StringUtils
{
    public static AList<LyricOverlayElement> SplitTextToLines(string text, double width, double height, Typeface typeface, TextAlignment alignment, double fontSize)
    {
        string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        AList<LyricOverlayElement> lines = new AList<LyricOverlayElement>();
        StringBuilder currentLine = new StringBuilder();

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            
            if (currentLine.Length > 0)
                currentLine.Append(' ');

            currentLine.Append(word);
            
            FormattedText formattedCandidateLine = new FormattedText(currentLine.ToString(), typeface, fontSize, alignment, TextWrapping.NoWrap, new Size(width, height));

            if (formattedCandidateLine.Bounds.Width > width)
            {
                currentLine.Length -= word.Length;
                if (currentLine.Length > 0 && char.IsWhiteSpace(currentLine[currentLine.Length - 1]))
                    currentLine.Length--;

                lines.Add(new LyricOverlayElement(currentLine.ToString(), 
                    MeasureSingleString(currentLine.ToString(), width, height, typeface, alignment, TextWrapping.NoWrap, fontSize)));
                currentLine.Clear();
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(new LyricOverlayElement(currentLine.ToString(), 
                MeasureSingleString(currentLine.ToString(), width, height, typeface, alignment, TextWrapping.NoWrap, fontSize)));
        }

        return lines;
    }
    
    public static Rect MeasureSingleString(string line, double width, double height, Typeface typeface, TextAlignment alignment, TextWrapping wrapping, double fontSize)
    {
        FormattedText formattedCandidateLine = new FormattedText(
            line, 
            typeface, 
            fontSize, 
            alignment, 
            wrapping, 
            new Size(width, height));

        return formattedCandidateLine.Bounds;
    }
}
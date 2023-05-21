using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;
using Avalonia.Media;
using DevBase.Generics;

namespace OpenLyricsClient.Frontend.Utils;

public class StringUtils
{
    public static AList<string> SplitTextToLines(string text, double width, double height, Typeface typeface, TextAlignment alignment, double fontSize)
    {
        var words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        var lines = new AList<string>();
        var currentLine = new StringBuilder();
        var textWrapping = TextWrapping.NoWrap;
        var constraint = new Size(width, height);

        foreach (var word in words)
        {
            if (currentLine.Length > 0)
                currentLine.Append(' ');

            currentLine.Append(word);
            var formattedCandidateLine = new FormattedText(currentLine.ToString(), typeface, fontSize, alignment, textWrapping, constraint);

            if (formattedCandidateLine.Bounds.Width > width)
            {
                // Remove the word that just got appended.
                currentLine.Length -= word.Length;
                if (currentLine.Length > 0 && char.IsWhiteSpace(currentLine[currentLine.Length - 1]))
                    currentLine.Length--; // Remove trailing space.

                lines.Add(currentLine.ToString());
                currentLine.Clear();
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            lines.Add(currentLine.ToString());
        }

        return lines;
    }

}
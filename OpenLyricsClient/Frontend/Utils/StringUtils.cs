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
        string[] words = text.Split(' ');
        
        AList<string> lines = new AList<string>();
        
        StringBuilder currentLine = new StringBuilder();
        
        TextWrapping textWrapping = TextWrapping.NoWrap;
        Size constraint = new Size(width, height);

        for (int i = 0; i < words.Length; i++)
        {
            string word = words[i];
            
            StringBuilder candidateLine = new StringBuilder(currentLine.ToString());
            
            if (candidateLine.Length > 0)
                candidateLine.Append(' ');
            candidateLine.Append(word);

            FormattedText formattedCandidateLine = new FormattedText(candidateLine.ToString(), typeface, fontSize, alignment, textWrapping, constraint);
            if (formattedCandidateLine.Bounds.Width <= width)
            {
                currentLine = candidateLine;
            }
            else
            {
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
using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Media;
using DevBase.Generics;
using OpenLyricsClient.UI.Structure;

namespace OpenLyricsClient.UI.Utils;

public class StringUtils
{
    public static async Task<AList<LyricOverlayElement>> SplitTextToLines(string text, double width, double height, Typeface typeface, TextAlignment alignment, double fontSize, Logic.Romanization.Romanization romanization = null)
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
            
            FormattedText formattedCandidateLine = new FormattedText(
                currentLine.ToString(), 
                CultureInfo.CurrentUICulture, 
                FlowDirection.LeftToRight, 
                typeface, 
                fontSize, 
                new SolidColorBrush(new Color(255,255,255,255)));
            
            if (formattedCandidateLine.Width > width)
            {
                currentLine.Length -= word.Length;
                if (currentLine.Length > 0 && char.IsWhiteSpace(currentLine[currentLine.Length - 1]))
                    currentLine.Length--;

                string line = currentLine.ToString();
                
                if (romanization != null)
                    line = await romanization.Romanize(line);
                
                lines.Add(new LyricOverlayElement(line, 
                    MeasureSingleString(line, typeface, fontSize)));
                currentLine.Clear();
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            string line = currentLine.ToString();
                
            if (romanization != null)
                line = await romanization.Romanize(line);
            
            lines.Add(new LyricOverlayElement(line, 
                MeasureSingleString(line, typeface, fontSize)));
        }

        return lines;
    }
    
    public static Size MeasureSingleString(string line, Typeface typeface, double fontSize)
    {
        FormattedText t = new FormattedText(
            line, 
            CultureInfo.CurrentUICulture, 
            FlowDirection.LeftToRight, 
            typeface, 
            fontSize, 
            new SolidColorBrush(new Color(255,255,255,255)));

        return new Size(t.Width, t.Height);
    }
}
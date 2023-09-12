using System;
using System.Collections.ObjectModel;
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
    public static async Task<ObservableCollection<LyricOverlayElement>> SplitTextToLines(string text, double width, Typeface typeface, double fontSize)
    {
        string[] words = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        ObservableCollection<LyricOverlayElement> lines = new ObservableCollection<LyricOverlayElement>();
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
                null);
            
            if (formattedCandidateLine.Width > width)
            {
                currentLine.Length -= word.Length;
                if (currentLine.Length > 0 && char.IsWhiteSpace(currentLine[currentLine.Length - 1]))
                    currentLine.Length--;

                string line = currentLine.ToString();
                
                lines.Add(new LyricOverlayElement(line, 
                    MeasureSingleString(line, typeface, fontSize)));
                
                currentLine.Clear();
                currentLine.Append(word);
            }
        }

        if (currentLine.Length > 0)
        {
            string line = currentLine.ToString();
                
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
            null);

        return new Size(t.Width, t.Height);
    }
}
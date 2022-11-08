using System;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace OpenLyricsClient.Backend.Romanization;

public class RomanizationConverter : IValueConverter
{
    public static readonly RomanizationConverter Instance = new();

    private Romanization _romanization;
    
    public RomanizationConverter()
    {
        _romanization = new Romanization();
    }
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string sourceText && 
            targetType.IsAssignableTo(typeof(string)))
        {

            string result = sourceText;
            
            Task t = Task.Factory.StartNew(async () =>
            {
                result = await _romanization.Romanize(sourceText);
            });

            t.Wait();

            return result;
        }
        
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
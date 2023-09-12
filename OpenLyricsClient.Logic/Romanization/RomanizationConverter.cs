using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace OpenLyricsClient.Logic.Romanization;

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
            return _romanization.Romanize(sourceText);
        }
        
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
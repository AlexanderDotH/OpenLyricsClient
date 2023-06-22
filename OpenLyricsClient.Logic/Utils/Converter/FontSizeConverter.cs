using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace OpenLyricsClient.Logic.Utils.Converter;

public class FontSizeConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int size)
        {
            if (size <= 0)
            {
                return 30;
            }
            else
            {
                return size;
            }
        }
        
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int size)
        {
            if (size <= 0)
            {
                return 30;
            }
            else
            {
                return size;
            }
        }
        
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
}
using System;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace OpenLyricsClient.Backend.Utils.Converter;

public class FontWeightConverter : IValueConverter
{
    public static readonly FontWeightConverter Instance = new();
    
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontWeight weight)
        {
            if (weight <= 0)
            {
                return FontWeight.Medium;
            }
            else
            {
                return weight;
            }
        }
        
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is FontWeight weight)
        {
            if (weight <= 0)
            {
                return FontWeight.Medium;
            }
            else
            {
                return weight;
            }
        }
        
        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }
}
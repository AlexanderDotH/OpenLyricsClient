using Avalonia;
using Avalonia.Media;
using Material.Styles;

namespace OpenLyricsClient.Frontend.Models.Elements;

public class RadioButton : Avalonia.Controls.RadioButton
{
    public static readonly StyledProperty<Brush> RippleBrushProperty =
        AvaloniaProperty.Register<RadioButton, Brush>(nameof(RippleBrush));
        
    public static readonly StyledProperty<double> RippleOpacityProperty =
        AvaloniaProperty.Register<RadioButton, double>(nameof(RippleOpacity));
    
    public static readonly StyledProperty<Brush> SelectionBrushProperty =
        AvaloniaProperty.Register<RadioButton, Brush>(nameof(SelectionBrush));

    public Brush SelectionBrush
    {
        get => GetValue(SelectionBrushProperty);
        set => SetValue(SelectionBrushProperty, value);
    }
    
    public double RippleOpacity
    {
        get => GetValue(RippleOpacityProperty);
        set => SetValue(RippleOpacityProperty, value);
    }
    
    public Brush RippleBrush
    {
        get => GetValue(RippleBrushProperty);
        set => SetValue(RippleBrushProperty, value);
    }
    
}
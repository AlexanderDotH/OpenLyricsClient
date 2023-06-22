using Avalonia;

namespace OpenLyricsClient.UI.Models.Elements;

public class MaterialCheckBox : Avalonia.Controls.CheckBox
{
    public static readonly StyledProperty<double> CheckBoxSizeProperty =
        AvaloniaProperty.Register<MaterialCheckBox, double>(nameof(CheckBoxSize));
    
    public static readonly StyledProperty<double> CheckBoxHoverSizeProperty =
        AvaloniaProperty.Register<MaterialCheckBox, double>(nameof(CheckBoxHoverSize));

    public double CheckBoxSize
    {
        get => GetValue(CheckBoxSizeProperty);
        set => SetValue(CheckBoxSizeProperty, value);
    }

    public double CheckBoxHoverSize
    {
        get => GetValue(CheckBoxHoverSizeProperty);
        set => SetValue(CheckBoxHoverSizeProperty, value);
    }
    
}
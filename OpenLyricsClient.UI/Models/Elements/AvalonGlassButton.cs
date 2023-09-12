using Avalonia;
using Avalonia.Controls;
using Material.Icons;

namespace OpenLyricsClient.UI.Models.Elements;

public class AvalonGlassButton : Button
{
    public static readonly StyledProperty<MaterialIconKind> IconKindProperty = AvaloniaProperty.Register<AvalonGlassButton, MaterialIconKind>(
        "IconKind");

    public MaterialIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }
}
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Material.Styles;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenLyricsClient.Frontend.Controls.Model;

public class GroupBox : Card
{
    public static readonly StyledProperty<Brush> HeaderBrushProperty =
        AvaloniaProperty.Register<Card, Brush>(nameof(HeaderBrush));
    
    public static readonly StyledProperty<string> HeaderTextProperty =
        AvaloniaProperty.Register<Card, string>(nameof(HeaderText));

    public Brush HeaderBrush
    {
        get => GetValue(HeaderBrushProperty);
        set => SetValue(HeaderBrushProperty, value);
    }
    
    public string HeaderText
    {
        get => GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }
    
}
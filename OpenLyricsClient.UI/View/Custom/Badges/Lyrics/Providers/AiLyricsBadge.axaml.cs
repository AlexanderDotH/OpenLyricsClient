using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace OpenLyricsClient.UI.View.Custom.Badges.Lyrics.Providers;

public partial class AiLyricsBadge : UserControl
{
    public static readonly StyledProperty<Color> StartColorProperty = AvaloniaProperty.Register<AiLyricsBadge, Color>(
        "StartColor");


    public static readonly StyledProperty<Color> EndColorProperty = AvaloniaProperty.Register<AiLyricsBadge, Color>(
        "EndColor");

    public static readonly StyledProperty<Brush> ForegroundColorBrushProperty = AvaloniaProperty.Register<AiLyricsBadge, Brush>(
        "ForegroundColorBrush");

    public AiLyricsBadge()
    {
        InitializeComponent();
        
        StartColor = Color.Parse("#0B666A");
        EndColor = Color.Parse("#97FEED");

        ForegroundColorBrush = new SolidColorBrush(Colors.White);
    }
    
    public Color StartColor
    {
        get => GetValue(StartColorProperty);
        set => SetValue(StartColorProperty, value);
    }
    
    public Brush ForegroundColorBrush
    {
        get => GetValue(ForegroundColorBrushProperty);
        set => SetValue(ForegroundColorBrushProperty, value);
    }
    
    public Color EndColor
    {
        get => GetValue(EndColorProperty);
        set => SetValue(EndColorProperty, value);
    }
}
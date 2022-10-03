using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using OpenLyricsClient.Frontend.View.Views;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class FullLyricsPage : UserControl
{
    public FullLyricsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.BeginMoveDrag(e);
    }
}
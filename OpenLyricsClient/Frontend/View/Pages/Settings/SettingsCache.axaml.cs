using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.Frontend.View.Pages.Settings;

public partial class SettingsCache : UserControl
{
    public SettingsCache()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
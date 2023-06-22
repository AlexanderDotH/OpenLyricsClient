using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.UI.View.Pages.Settings;

public partial class SettingsLyrics : UserControl
{
    public SettingsLyrics()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
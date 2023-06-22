using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.UI.View.Pages.Settings;

public partial class SettingsCredits : UserControl
{
    public SettingsCredits()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
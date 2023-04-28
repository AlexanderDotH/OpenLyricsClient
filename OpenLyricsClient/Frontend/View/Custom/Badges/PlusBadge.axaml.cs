using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.Frontend.View.Custom.Badges;

public partial class PlusBadge : UserControl
{
    public PlusBadge()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
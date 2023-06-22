using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.UI.View.Custom.Badges;

public partial class DefaultBadge : UserControl
{
    public DefaultBadge()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
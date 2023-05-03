using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.Frontend.View.Custom.Badges.Other;

public partial class DesignerBadge : UserControl
{
    public DesignerBadge()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenLyricsClient.UI.View.Custom.Badges.Other;

public partial class TesterBadge : UserControl
{
    public TesterBadge()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
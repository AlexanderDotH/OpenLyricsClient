using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace OpenLyricsClient.Frontend.View.Pages.Settings.Providers;

public partial class SettingsSpotify : UserControl
{
    private Grid _gridProfile;
    
    public SettingsSpotify()
    {
        InitializeComponent();

        this._gridProfile = this.Get<Grid>(nameof(GRD_Profile));
        
        Image image = new Image();
        image.Width = 70;
        image.Height = 70;
        image.VerticalAlignment = VerticalAlignment.Center;
        image.HorizontalAlignment = HorizontalAlignment.Left;
        image.Margin = new Thickness(0, 0, 0, 60);

        SolidColorBrush primaryBackColor = App.Current.FindResource("PrimaryBackgroundBrush") as SolidColorBrush;
        
        Border border = new Border();
        border.Width = 78;
        border.Height = 78;
        border.VerticalAlignment = VerticalAlignment.Center;
        border.HorizontalAlignment = HorizontalAlignment.Left;
        border.Margin = new Thickness(0, 0, 0, 60);
        border.BorderThickness = new Thickness(5);
        border.BorderBrush = primaryBackColor;
        border.CornerRadius = new CornerRadius(8);
        
        this._gridProfile.Children.Add(image);
        this._gridProfile.Children.Add(border);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
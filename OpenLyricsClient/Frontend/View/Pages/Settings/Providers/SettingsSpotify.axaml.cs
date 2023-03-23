using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DevBase.Web;
using MusixmatchClientLib.Web.ResponseData;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using ResponseData = DevBase.Web.ResponseData.ResponseData;

namespace OpenLyricsClient.Frontend.View.Pages.Settings.Providers;

public partial class SettingsSpotify : UserControl
{
    private Grid _gridProfile;

    private Image _profileImage;
    
    public SettingsSpotify()
    {
        InitializeComponent();

        this._gridProfile = this.Get<Grid>(nameof(GRD_Profile));
        
        Image image = new Image();
        image.Width = 70;
        image.Height = 70;
        image.VerticalAlignment = VerticalAlignment.Center;
        image.HorizontalAlignment = HorizontalAlignment.Left;
        image.Margin = new Thickness(15, 5, 0, 60);

        this._profileImage = image;

        SolidColorBrush primaryBackColor = App.Current.FindResource("SecondaryBackgroundBrush") as SolidColorBrush;
        
        Border border = new Border();
        border.Width = 78;
        border.Height = 78;
        border.VerticalAlignment = VerticalAlignment.Center;
        border.HorizontalAlignment = HorizontalAlignment.Left;
        border.Margin = new Thickness(10, 5, 0, 60);
        border.BorderThickness = new Thickness(5);
        border.BorderBrush = primaryBackColor;
        border.CornerRadius = new CornerRadius(8);

        Core.INSTANCE.SettingManager.SettingsChanged += SettingManagerOnSettingsChanged;
        
        //image.Source = new Bitmap("C:\\Users\\alexa\\Desktop\\ab6775700000ee85216a8ba62f36357fee22d1d5.jpg");
        ProfileImageUpdate();

        this._gridProfile.Children.Add(image);
        this._gridProfile.Children.Add(border);
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        ProfileImageUpdate();
    }

    private void ProfileImageUpdate()
    {
        Task.Factory.StartNew(async() =>
        {
            if (!Core.INSTANCE!.ServiceHandler.IsConnected("Spotify"))
                return;
            
            Request request =
                new Request(Core.INSTANCE?.SettingManager?.Settings?.SpotifyAccess?.UserData?.Images[0]?.Url!);
            ResponseData responseData = await request.GetResponseAsync();
                
            MemoryStream ms = new MemoryStream(responseData.Content);

            await Dispatcher.UIThread.InvokeAsync(() => this._profileImage.Source = new Bitmap(ms));
        });
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
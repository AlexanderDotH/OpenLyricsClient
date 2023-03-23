using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using DevBase.Generics;
using DevBase.Web;
using MusixmatchClientLib.Web.ResponseData;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Frontend.Models.Elements;
using OpenLyricsClient.Frontend.Structure;
using SpotifyAPI.Web;
using Image = Avalonia.Controls.Image;
using ResponseData = DevBase.Web.ResponseData.ResponseData;
using SimpleArtist = OpenLyricsClient.Backend.Structure.Other.SimpleArtist;
using SimpleTrack = OpenLyricsClient.Backend.Structure.Other.SimpleTrack;

namespace OpenLyricsClient.Frontend.View.Pages.Settings.Providers;

public partial class SettingsSpotify : UserControl
{
    private Grid _gridProfile;

    private Image _profileImage;

    private ListBox _topArtists;
    private ListBox _topTracks;
    
    public SettingsSpotify()
    {
        InitializeComponent();

        this._gridProfile = this.Get<Grid>(nameof(GRD_Profile));
        
        Image image = new Image();
        image.Width = 70;
        image.Height = 70;
        image.VerticalAlignment = VerticalAlignment.Center;
        image.HorizontalAlignment = HorizontalAlignment.Left;
        image.Margin = new Thickness(15, 5, 0, 10);

        this._profileImage = image;

        SolidColorBrush primaryBackColor = App.Current.FindResource("SecondaryBackgroundBrush") as SolidColorBrush;
        
        Border border = new Border();
        border.Width = 78;
        border.Height = 78;
        border.VerticalAlignment = VerticalAlignment.Center;
        border.HorizontalAlignment = HorizontalAlignment.Left;
        border.Margin = new Thickness(10, 5, 0, 10);
        border.BorderThickness = new Thickness(5);
        border.BorderBrush = primaryBackColor;
        border.CornerRadius = new CornerRadius(8);

        Core.INSTANCE.SettingManager.SettingsChanged += SettingManagerOnSettingsChanged;
        
        //image.Source = new Bitmap("C:\\Users\\alexa\\Desktop\\ab6775700000ee85216a8ba62f36357fee22d1d5.jpg");
        ProfileImageUpdate();
        LoadStats();
        
        this._gridProfile.Children.Add(image);
        this._gridProfile.Children.Add(border);

        this._topArtists = this.Get<ListBox>(nameof(LST_TopArtists));
        this._topTracks = this.Get<ListBox>(nameof(LST_TopTracks));
    }

    private void SettingManagerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        ProfileImageUpdate();
        LoadStats();
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

    private void LoadStats()
    {
        Task.Factory.StartNew(async () =>
        {
            SimpleArtist[] artists = Core.INSTANCE?.SettingManager?.Settings?.SpotifyAccess?.Statistics?.TopArtists!;

            AList<ListBoxElement> elements = new AList<ListBoxElement>();

            for (int i = 0; i < artists.Length; i++)
            {
                SimpleArtist artist = artists[i];

                if (artist.Images.Length <= 2)
                    continue;

                Request request = new Request(artist.Images[2].Url);
                ResponseData data = await request.GetResponseAsync();
                MemoryStream buffer = new MemoryStream(data.Content);
                
                ListBoxElement element = new ListBoxElement();
                element.Text = artist.Name;
                element.Image = new Bitmap(buffer);
                
                elements.Add(element);
            }
            
            await Dispatcher.UIThread.InvokeAsync(() => this._topArtists.Items = elements.GetAsList());
        });
        
        Task.Factory.StartNew(async () =>
        {
            SimpleTrack[] tracks = Core.INSTANCE?.SettingManager?.Settings?.SpotifyAccess?.Statistics?.TopTracks!;

            AList<ListBoxElement> elements = new AList<ListBoxElement>();

            for (int i = 0; i < tracks.Length; i++)
            {
                SimpleTrack track = tracks[i];

                if (track.Cover.Length <= 2)
                    continue;

                Request request = new Request(track.Cover[2].Url);
                ResponseData data = await request.GetResponseAsync();
                MemoryStream buffer = new MemoryStream(data.Content);
                
                ListBoxElement element = new ListBoxElement();
                element.Text = track.Name;
                element.Image = new Bitmap(buffer);
                
                elements.Add(element);
            }
            
            await Dispatcher.UIThread.InvokeAsync(() => this._topTracks.Items = elements.GetAsList());
        });
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
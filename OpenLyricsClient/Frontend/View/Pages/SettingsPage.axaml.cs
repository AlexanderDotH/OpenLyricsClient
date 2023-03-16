using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Rendering;
using Avalonia.Threading;
using DevBase.Generics;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Frontend.View.Windows;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class SettingsPage : UserControl
{
    private Button _btnRomanization;
    private Button _btnLyrics;
    private Button _btnSpotify;
    private Button _btnCache;
    
    private Carousel _pageSelector;

    private ATupleList<Button, int> _buttonList;
    
    private bool _loaded;

    private int _currentIndex;
    private int _oldIndex;
    
    public SettingsPage()
    {
        InitializeComponent();

        this._loaded = false;

        this._buttonList = new ATupleList<Button, int>();

        this._btnRomanization = this.Get<Button>(nameof(BTN_Romanization));
        this._btnLyrics = this.Get<Button>(nameof(BTN_Lyrics));
        this._btnSpotify = this.Get<Button>(nameof(BTN_Spotify));
        this._btnCache = this.Get<Button>(nameof(BTN_Cache));
        
        this._buttonList.AddRange(
            new Tuple<Button, int>(this._btnRomanization, 0), 
            new Tuple<Button, int>(this._btnLyrics, 1), 
            new Tuple<Button, int>(this._btnSpotify, 2), 
            new Tuple<Button, int>(this._btnCache, 3));
        
        this._pageSelector = this.Get<Carousel>(nameof(PageSelection));
        
        this._oldIndex = -1;
        
        SelectPage(0);

        UiThreadRenderTimer uiThreadRenderTimer = new UiThreadRenderTimer(60);
        uiThreadRenderTimer.Tick += span =>
        {
            if (this._currentIndex != this._oldIndex || this._oldIndex < 0)
            {
                Button element = this._buttonList.FindEntry(_currentIndex);
        
                element.Foreground = App.Current.FindResource("PrimaryThemeFontColorBrush") as SolidColorBrush;
                element.Background = App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
                this._oldIndex = this._currentIndex;
            }
        };
        
        /*Task.Factory.StartNew(async () =>
        {
           while (!Core.IsDisposed())
           {
               await Task.Delay(500);

               await Dispatcher.UIThread.InvokeAsync(() =>
               {
                   this._connectToSpotify.IsEnabled = !Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected;
                   this._disconnectFromSpotify.IsEnabled = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected;

                   if (Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected)
                   {
                       this._txtSpotify.Text = "Welcome " + Core.INSTANCE.SettingManager.Settings.SpotifyAccess.UserData.DisplayName;
                   }
                   else
                   {
                       this._txtSpotify.Text = "Connect with Spotify";
                   }

                   this._japaneseToRomanji.IsChecked =
                       Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection
                           .JAPANESE_TO_ROMANJI);
                   
                   this._koreanToRomanji.IsChecked =
                       Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection
                           .KOREAN_TO_ROMANJI);
                   
                   this._russiaToLatin.IsChecked =
                       Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection
                           .RUSSIA_TO_LATIN);
                   
               });
           }
       });*/
        
        this._loaded = true;

    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        MainWindow.Instance.BeginMoveDrag(e);
    }

    /*private void BTN_ConnectSpotify_OnClick(object? sender, RoutedEventArgs e)
    {
        Core.INSTANCE.ServiceHandler.AuthorizeService("Spotify");
    }

    private void BTN_DisconnectSpotify_OnClick(object? sender, RoutedEventArgs e)
    {
        Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected = false;
        Core.INSTANCE.SettingManager.Settings.SpotifyAccess.AccessToken = "";
        Core.INSTANCE.SettingManager.Settings.SpotifyAccess.RefreshToken = "";
        Core.INSTANCE.SettingManager.WriteSettings();
    }

    private void BTN_ClearCache_OnClick(object? sender, RoutedEventArgs e)
    {
        Core.INSTANCE.CacheManager.ClearCache();
    }

    private void BTN_RefreshLyrics_OnClick(object? sender, RoutedEventArgs e)
    {
        Core.INSTANCE.SongHandler.RequestNewSong();
    }

    private void CHK_JapaneseToRomanji_OnChecked(object? sender, RoutedEventArgs e)
    {
        if (!this._loaded)
            return;
        
        if (!this._japaneseToRomanji.IsChecked.HasValue)
            return;

        if (!Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.JAPANESE_TO_ROMANJI))
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Add(RomanizeSelection.JAPANESE_TO_ROMANJI);
            Core.INSTANCE.SettingManager.WriteSettings();
        }
    }

    private void CHK_JapaneseToRomanji_OnUnchecked(object? sender, RoutedEventArgs e)
    {
        if (!this._loaded)
            return;
        
        if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.JAPANESE_TO_ROMANJI))
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Remove(RomanizeSelection.JAPANESE_TO_ROMANJI);
            Core.INSTANCE.SettingManager.WriteSettings();
        }
    }
    
    private void CHK_KoreanToRomanji_OnChecked(object? sender, RoutedEventArgs e)
    {
        if (!this._loaded)
            return;
        
        if (!this._koreanToRomanji.IsChecked.HasValue)
            return;

        if (!Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.KOREAN_TO_ROMANJI))
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Add(RomanizeSelection.KOREAN_TO_ROMANJI);
            Core.INSTANCE.SettingManager.WriteSettings();
        }
    }
    
    private void CHK_KoreanToRomanji_OnUnchecked(object? sender, RoutedEventArgs e)
    {
        if (!this._loaded)
            return;
        
        if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.KOREAN_TO_ROMANJI))
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Remove(RomanizeSelection.KOREAN_TO_ROMANJI);
            Core.INSTANCE.SettingManager.WriteSettings();
        }
    }

    private void CHK_RussiaToLatin_OnChecked(object? sender, RoutedEventArgs e)
    {
        if (!this._loaded)
            return;
        
        if (!this._russiaToLatin.IsChecked.HasValue)
            return;

        if (!Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.RUSSIA_TO_LATIN))
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Add(RomanizeSelection.RUSSIA_TO_LATIN);
            Core.INSTANCE.SettingManager.WriteSettings();
        }
    }

    private void CHK_RussiaToLatin_OnUnchecked(object? sender, RoutedEventArgs e)
    {
        if (!this._loaded)
            return;
        
        if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.RUSSIA_TO_LATIN))
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Remove(RomanizeSelection.RUSSIA_TO_LATIN);
            Core.INSTANCE.SettingManager.WriteSettings();
        }
    }*/

    private void UnselectAll()
    {
        this._buttonList.ForEach(b =>
        {
            b.Item1.Foreground = App.Current.FindResource("SecondaryFontColorBrush") as SolidColorBrush;
            b.Item1.Background = App.Current.FindResource("SecondaryBackgroundBrush") as SolidColorBrush;
        });
    }

    private void SelectPage(int index)
    {
        this._oldIndex = -1;
        this._currentIndex = index;
        
        UnselectAll();
        this._pageSelector.SelectedIndex = index;
    }

    private void BTN_Romanization_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(0);

        Song currentSong = Core.INSTANCE.SongHandler.CurrentSong;
        
        DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient api =
            new DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient();
        
        api.SubmitAiSync(currentSong.SongMetadata.Name, currentSong.SongMetadata.Album,
            currentSong.SongMetadata.MaxTime, "large-v2", currentSong.SongMetadata.Artists).GetAwaiter().GetResult();
        
        Core.INSTANCE.ServiceHandler.AuthorizeService("Spotify");
    }

    private void BTN_Lyrics_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(1);
    }

    private void BTN_Spotify_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(2);
    }

    private void BTN_Cache_OnClick(object? sender, RoutedEventArgs e)
    {
        SelectPage(3);
    }
}
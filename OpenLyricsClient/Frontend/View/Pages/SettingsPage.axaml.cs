using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Frontend.Models.Pages;
using OpenLyricsClient.Frontend.View.Windows;
using SelectionMode = OpenLyricsClient.Backend.Structure.Enum.SelectionMode;

namespace OpenLyricsClient.Frontend.View.Pages;

public partial class SettingsPage : UserControl
{
    private ComboBox _comboboxMode;

    private Button _connectToSpotify;
    private Button _disconnectFromSpotify;
    private TextBlock _txtSpotify;

    private CheckBox _japaneseToRomanji;
    private CheckBox _koreanToRomanji;
    private CheckBox _russiaToLatin;

    private bool _loaded;
    
    public SettingsPage()
    {
        InitializeComponent();

        this._loaded = false;
        
        this._comboboxMode = this.Get<ComboBox>(nameof(CMBX_LyricsSelection));

        this._connectToSpotify = this.Get<Button>(nameof(BTN_ConnectSpotify));
        this._disconnectFromSpotify = this.Get<Button>(nameof(BTN_DisconnectSpotify));
        this._txtSpotify = this.Get<TextBlock>(nameof(TXT_ConnectSpotify));
        
        this._japaneseToRomanji = this.Get<CheckBox>(nameof(CHK_JapaneseToRomanji));
        this._koreanToRomanji = this.Get<CheckBox>(nameof(CHK_KoreanToRomanji));
        this._russiaToLatin = this.Get<CheckBox>(nameof(CHK_RussiaToLatin));
        
        Task.Factory.StartNew(async () =>
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

                   if (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.QUALITY)
                   {
                       this._comboboxMode.SelectedIndex = 0;
                   } else if (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.PERFORMANCE)
                   {
                       this._comboboxMode.SelectedIndex = 1;
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
       });
        
        this._loaded = true;

    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!this._comboboxMode.IsDropDownOpen)
            MainWindow.Instance.BeginMoveDrag(e);
    }

    private void BTN_ConnectSpotify_OnClick(object? sender, RoutedEventArgs e)
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

    private void CMBX_LyricsSelection_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (!this._loaded)
            return;
        
        if (this._comboboxMode.SelectedItem is ComboBoxItem)
        {
            ComboBoxItem selectedItem = (ComboBoxItem)this._comboboxMode.SelectedItem;

            if (selectedItem.Content is string)
            {
                string selectedText = (string)selectedItem.Content;

                switch (selectedText)
                {
                    case "Quality":
                    {
                        Core.INSTANCE.SettingManager.Settings.LyricSelectionMode = SelectionMode.QUALITY;
                        Core.INSTANCE.SettingManager.WriteSettings();
                        break;
                    }
                    case "Performance":
                    {
                        Core.INSTANCE.SettingManager.Settings.LyricSelectionMode = SelectionMode.PERFORMANCE;
                        Core.INSTANCE.SettingManager.WriteSettings();
                        break;
                    }
                }
            }
        }
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
    }
}
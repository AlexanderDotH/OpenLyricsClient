using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LyricsWPF.Backend;
using LyricsWPF.Backend.Romanisation;
using LyricsWPF.Backend.Settings;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Frontend.ItemSources;
using MaterialDesignThemes.Wpf;
using SelectionMode = LyricsWPF.Backend.Collector.SelectionMode;
using Window = System.Windows.Window;

namespace LyricsWPF
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {

        public Settings()
        {
            InitializeComponent();

            this.btnSpotifyDisconnect.IsEnabled = false;
            this.btnYoutubeDisconnect.IsEnabled = false;
            this.btnTidalDisconnect.IsEnabled = false;

            if (Core.INSTANCE.ServiceHandler.IsConnected("Spotify"))
            {
                this.btnSpotify.IsEnabled = false;
                this.btnSpotifyDisconnect.IsEnabled = true;
                this.btnSpotify.Content = "Connected";
            }

            if (Core.INSTANCE.ServiceHandler.IsConnected("Tidal"))
            {
                this.btnTidal.IsEnabled = false;
                this.btnTidalDisconnect.IsEnabled = true;
                this.btnTidal.Content = "Connected";
            }

            LyricsProviderItemSource lyricsProviderItemSource = new LyricsProviderItemSource();

            lscb.ItemsSource = lyricsProviderItemSource.ListOfItems;

            SelectionMode selectionMode = Core.INSTANCE.SettingManager.Settings.LyricSelectionMode;

            if (selectionMode == SelectionMode.PERFORMANCE)
                lscb.SelectedItem = "Performance";

            if (selectionMode == SelectionMode.QUALITY)
                lscb.SelectedItem = "Quality";

            this.chkJtR.IsChecked = Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.JAPANESE_TO_ROMANJI);
            this.chkKtR.IsChecked = Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.KOREAN_TO_ROMANJI);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Core.INSTANCE.ServiceHandler.AuthorizeService("Spotify");

            int spotifyCheckTime = 0;

            Thread check = new Thread(() =>
            {
                while (spotifyCheckTime != 10)
                {
                    Thread.Sleep(2000);
                    if (Core.INSTANCE.ServiceHandler.IsConnected("Spotify"))
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            this.btnSpotify.IsEnabled = false;
                            this.btnSpotify.Content = "Connected";
                            this.btnSpotifyDisconnect.IsEnabled = true;
                        }));
                    }

                    spotifyCheckTime++;
                }
            });
            check.Start();

        }

        private void btnTidal_Click(object sender, RoutedEventArgs e)
        {
            Core.INSTANCE.ServiceHandler.AuthorizeService("Tidal");

            int tidalCheckTime = 0;

            Thread check = new Thread(() =>
            {
                while (tidalCheckTime != 10)
                {
                    Thread.Sleep(2000);
                    if (Core.INSTANCE.ServiceHandler.IsConnected("Tidal"))
                    {
                        this.Dispatcher.Invoke((Action)(() =>
                        {
                            this.btnTidal.IsEnabled = false;
                            this.btnTidalDisconnect.IsEnabled = true;
                            this.btnTidal.Content = "Connected";
                        }));
                    }

                    tidalCheckTime++;
                }
            });
            check.Start();
        }

        private void lscb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selection = this.lscb.SelectedItem.ToString();

            if (selection.Equals("Quality"))
            {
                Core.INSTANCE.SettingManager.Settings.LyricSelectionMode = SelectionMode.QUALITY;
            } 
            else if (selection.Equals("Performance"))
            {
                Core.INSTANCE.SettingManager.Settings.LyricSelectionMode = SelectionMode.PERFORMANCE;
            }

            Core.INSTANCE.SettingManager.WriteSettings();
        }

        private void chkJtR_Checked(object sender, RoutedEventArgs e)
        {
            if (!Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.JAPANESE_TO_ROMANJI))
            {
                Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Add(RomanizeSelection.JAPANESE_TO_ROMANJI);
                Core.INSTANCE.SettingManager.WriteSettings();
            }
        }

        private void chkJtR_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection
                    .JAPANESE_TO_ROMANJI))
            {
                Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Remove(RomanizeSelection.JAPANESE_TO_ROMANJI);
                Core.INSTANCE.SettingManager.WriteSettings();
            }
        }

        private void chkKtR_Checked(object sender, RoutedEventArgs e)
        {
            if (!Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection.KOREAN_TO_ROMANJI))
            {
                Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Add(RomanizeSelection.KOREAN_TO_ROMANJI);
                Core.INSTANCE.SettingManager.WriteSettings();
            }
        }

        private void chkKtR_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(RomanizeSelection
                    .KOREAN_TO_ROMANJI))
            {
                Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Remove(RomanizeSelection.KOREAN_TO_ROMANJI);
                Core.INSTANCE.SettingManager.WriteSettings();
            }
        }

        private void ButtonSpotifyDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess =
                (SpotifyAccess)Core.INSTANCE.SettingManager.DefaultSetting(EnumSetting.SPOTIFY);
            Core.INSTANCE.SettingManager.WriteSettings();

            this.btnSpotify.IsEnabled = true;
            this.btnSpotifyDisconnect.IsEnabled = false;
        }

        private void ButtonTidalDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Core.INSTANCE.SettingManager.Settings.TidalAccess =
                (TidalAccess)Core.INSTANCE.SettingManager.DefaultSetting(EnumSetting.TIDAL);
            Core.INSTANCE.SettingManager.WriteSettings();

            this.btnTidal.IsEnabled = true;
            this.btnTidalDisconnect.IsEnabled = false;
        }

        private void ButtonYoutubeDisconnect_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

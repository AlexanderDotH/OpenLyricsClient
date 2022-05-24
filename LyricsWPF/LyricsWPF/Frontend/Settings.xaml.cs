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
using LyricsWPF.Backend.Settings;
using LyricsWPF.Frontend.ItemSources;
using SelectionMode = LyricsWPF.Backend.Collector.SelectionMode;

namespace LyricsWPF
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private int spotifyCheckTime;

        public Settings()
        {
            InitializeComponent();

            spotifyCheckTime = 0;

            if (Core.INSTANCE.ServiceHandler.IsConnected("Spotify"))
            {
                this.btnSpotify.IsEnabled = false;
                this.btnSpotify.Content = "Connected";
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
                        }));
                    }

                    spotifyCheckTime++;
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
    }
}

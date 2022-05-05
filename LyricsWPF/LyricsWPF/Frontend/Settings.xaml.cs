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
    }
}

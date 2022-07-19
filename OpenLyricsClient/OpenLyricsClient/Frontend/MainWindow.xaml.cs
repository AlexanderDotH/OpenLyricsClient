using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DevBase.Async.Task;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Romanisation;
using OpenLyricsClient.Backend.Settings;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.ItemSources;
using SelectionMode = OpenLyricsClient.Backend.Collector.Lyrics.SelectionMode;

namespace OpenLyricsClient.Frontend
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private Settings _settings;

        private TaskSuspensionToken _showInfoSuspensionToken;
        private TaskSuspensionToken _showLyricSuspensionToken;
        private TaskSuspensionToken _showProgressSuspensionToken;

        public MainWindow()
        {
            InitializeComponent();

            SetupMainActivity();
            SetupSettingActivity();
        }

        #region Window

        private void SetupMainActivity()
        {
            this.fullLyricText.Text = @"";

            this.firstLine.Text = string.Empty;
            this.secondLine.Text = string.Empty;
            this.thirdLine.Text = string.Empty;
            this.fourthLine.Text = string.Empty;
            this.fifthLine.Text = string.Empty;
            this.provider.Text = string.Empty;
            this.currentTitle.Text = string.Empty;
            this.currentArtists.Text = string.Empty;
            this.provider.Text = string.Empty;
            this.fullLyricText.Text = string.Empty;

            this.pgSongProgress.Value = 0;

            Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;

            Core.INSTANCE.TaskRegister.RegisterTask(
                out this._showInfoSuspensionToken,
                new Task(async () => await this.ShowInfoTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SHOW_INFOS);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out this._showLyricSuspensionToken,
                new Task(async () => await this.ShowLyricsTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SHOW_LYRICS);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out this._showProgressSuspensionToken,
                new Task(async () => await this.ShowProgressTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                EnumRegisterTypes.SHOW_PROGRESS);
        }

        private async Task ShowProgressTask()
        {
            while (!Core.IsDisposed())
            {
                await Task.Delay(250);
                await this._showProgressSuspensionToken.WaitForRelease();

                Song song = Core.INSTANCE.SongHandler.CurrentSong;

                if (!DataValidator.ValidateData(song))
                    continue;

                if (!DataValidator.ValidateData(song.Time, song.MaxTime))
                    continue;

                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.pgSongProgress.Value = song.GetPercentage();
                });
            }
        }

        private async Task ShowInfoTask()
        {
            while (!Core.IsDisposed())
            {
                await this._showInfoSuspensionToken.WaitForRelease();
                await Task.Delay(100);

                Song song = Core.INSTANCE.SongHandler.CurrentSong;

                if (!DataValidator.ValidateData(song))
                    continue;

                if (!DataValidator.ValidateData(song.Title, song.ProgressString, song.MaxProgressString))
                    continue;

                //Name
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.currentTitle.Text = song.Title;
                    this.currentFullTitle.Text = song.Title;
                    this.currentArtists.Text = song.FullArtists;

                    this.timeFrom.Text = song.ProgressString;
                    this.timeTo.Text = song.MaxProgressString;
                });

                if (!DataValidator.ValidateData(song.Lyrics))
                    continue;

                if (!DataValidator.ValidateData(song.State))
                    continue;

                if (!DataValidator.ValidateData(song.Lyrics.LyricProvider))
                    continue;

                if (song.State == SongState.SEARCHING_LYRICS)
                    continue;

                if (song.State == SongState.HAS_LYRICS_AVAILABLE)
                {
                    //Lyric provider
                    this.Dispatcher.Invoke(() =>
                    {
                        this.provider.Text = "Powered by " + song.Lyrics.LyricProvider;
                        this.fullLyricText.Text = song.Lyrics.FullLyrics;
                    });
                }
            }
        }

        private async Task ShowLyricsTask()
        {
            while (!Core.IsDisposed())
            {
                await this._showLyricSuspensionToken.WaitForRelease();
                await Task.Delay(50);

                Song song = Core.INSTANCE.SongHandler.CurrentSong;

                if (!DataValidator.ValidateData(song))
                    continue;

                if (!DataValidator.ValidateData(song.State))
                    continue;

                //if (song.State != SongState.HAS_LYRICS_AVAILABLE)
                //{
                //    await this.Dispatcher.InvokeAsync(() =>
                //    {
                //        this.firstLine.Text = "";
                //        this.secondLine.Text = "";
                //        this.thirdLine.Text = song.State.ToString();
                //        this.fourthLine.Text = "";
                //        this.fifthLine.Text = "";
                //    });
                //}

                if (song.State == SongState.SEARCHING_LYRICS)
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "";
                        this.thirdLine.Text = "Searching lyrics...";
                        this.fourthLine.Text = "";
                        this.fifthLine.Text = "";
                    });
                }

                if (song.State == SongState.NO_LYRICS_AVAILABLE)
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "";
                        this.thirdLine.Text = "Lyrics not found";
                        this.fourthLine.Text = "";
                        this.fifthLine.Text = "";
                    });
                }

                if (song.State == SongState.HAS_LYRICS_AVAILABLE && !DataValidator.ValidateData(song.CurrentLyricsRoll))
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "";
                        this.thirdLine.Text = "♪";
                        this.fourthLine.Text = "";
                        this.fifthLine.Text = "";
                    });
                }

                if (!DataValidator.ValidateData(song.Lyrics) &&
                    DataValidator.ValidateData(song.Lyrics.LyricType) &&
                    DataValidator.ValidateData(song.CurrentLyricsRoll))
                    continue;

                if (song.Lyrics.LyricType == LyricType.INSTRUMENTAL)
                {
                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "";
                        this.thirdLine.Text = "♪ Instrumental ♪";
                        this.fourthLine.Text = "";
                        this.fifthLine.Text = "";
                    });
                }

                if (song.Lyrics.LyricType == LyricType.TEXT)
                {
                    LyricsRoll lyricsRoll = song.CurrentLyricsRoll;

                    if (!DataValidator.ValidateData(lyricsRoll))
                        continue;

                    await this.Dispatcher.InvokeAsync(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "";
                        this.thirdLine.Text = "♪";
                        this.fourthLine.Text = "";
                        this.fifthLine.Text = "";

                        if (DataValidator.ValidateData(lyricsRoll.PartOne) &&
                            DataValidator.ValidateData(lyricsRoll.PartOne.Part))
                        {
                            this.firstLine.Text = lyricsRoll.PartOne.Part;
                        }
                        else
                        {
                            this.firstLine.Text = "";
                        }

                        if (DataValidator.ValidateData(lyricsRoll.PartTwo) &&
                            DataValidator.ValidateData(lyricsRoll.PartTwo.Part))
                        {
                            this.secondLine.Text = lyricsRoll.PartTwo.Part;
                        }
                        else
                        {
                            this.secondLine.Text = "";
                        }

                        if (DataValidator.ValidateData(lyricsRoll.PartThree) &&
                            DataValidator.ValidateData(lyricsRoll.PartThree.Part))
                        {
                            this.thirdLine.Text = lyricsRoll.PartThree.Part;
                        }
                        else
                        {
                            this.thirdLine.Text = "";
                        }

                        if (DataValidator.ValidateData(lyricsRoll.PartFour) &&
                            DataValidator.ValidateData(lyricsRoll.PartFour.Part))
                        {
                            this.fourthLine.Text = lyricsRoll.PartFour.Part;
                        }
                        else
                        {
                            this.fourthLine.Text = "";
                        }

                        if (DataValidator.ValidateData(lyricsRoll.PartFive) &&
                            DataValidator.ValidateData(lyricsRoll.PartFive.Part))
                        {
                            this.fifthLine.Text = lyricsRoll.PartFive.Part;
                        }
                        else
                        {
                            this.fifthLine.Text = "";
                        }
                    });
                }
            }
        }

        private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
        {
            if (songchangedevent.EventType != EventType.POST)
                return;

            this.Dispatcher.Invoke(() =>
            {
                this.firstLine.Text = "";
                this.secondLine.Text = "";
                this.thirdLine.Text = "";
                this.fourthLine.Text = "";
                this.fifthLine.Text = "";
                this.provider.Text = "";
                this.currentTitle.Text = "";
                this.currentArtists.Text = "";
                this.provider.Text = "";
                this.fullLyricText.Text = "";
            });
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (this._settings == null)
        //    {
        //        this._settings = new Settings();
        //        this._settings.Closed += (o, args) => this._settings = null;
        //        this._settings.Show();
        //    }
        //    else
        //    {
        //        this._settings.Show();
        //    }
        //}

        #endregion


        #region Settings

        private void SetupSettingActivity()
        {

            this.btnSpotifyDisconnect.IsEnabled = false;
            this.btnYoutubeDisconnect.IsEnabled = false;
            this.btnTidalDisconnect.IsEnabled = false;

            this.btnYoutube.IsEnabled = false;

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

        private void btnSpotifyConnect_Click(object sender, RoutedEventArgs e)
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

        private void btnTidalConnect_Click(object sender, RoutedEventArgs e)
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

        private void btnClearCache_Click(object sender, RoutedEventArgs e)
        {
            Core.INSTANCE.CacheManager.ClearCache();

            this.firstLine.Text = "";
            this.secondLine.Text = "";
            this.thirdLine.Text = "";
            this.fourthLine.Text = "";
            this.fifthLine.Text = "";
        }

        #endregion


        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core.INSTANCE.DisposeEverything();
        }
    }
}

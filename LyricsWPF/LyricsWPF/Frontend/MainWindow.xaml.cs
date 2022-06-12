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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using DevBase.Async.Task;
using LyricsWPF.Backend;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF
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

            this.fullLyricText.Text = @"";

            this.currentFullTitle.Text = string.Empty;
            this.currentTitle.Text = string.Empty;
            this.provider.Text = string.Empty;

            this.firstLine.Text = string.Empty;
            this.secondLine.Text = string.Empty;
            this.thirdLine.Text = string.Empty;

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
                await Task.Delay(1);
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

                //Title
                await this.Dispatcher.InvokeAsync(() =>
                {
                    this.currentTitle.Text = song.Title;
                    this.currentFullTitle.Text = song.Title;

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
                    await this.Dispatcher.InvokeAsync(() =>
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
                await Task.Delay(1);

                Song song = Core.INSTANCE.SongHandler.CurrentSong;

                if (!DataValidator.ValidateData(song))
                    continue;

                if (!DataValidator.ValidateData(song.State))
                    continue;

                if (song.State == SongState.NO_LYRICS_AVAILABLE)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "Lyrics not found";
                        this.thirdLine.Text = "";
                    });
                }
                else if (song.State == SongState.SEARCHING_LYRICS)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "Searching lyrics...";
                        this.thirdLine.Text = "";
                    });
                }
                else if (song.State == SongState.HAS_LYRICS_AVAILABLE || song.State == SongState.SEARCHING_FINISHED)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        this.firstLine.Text = "";
                        this.secondLine.Text = "♪";
                        this.thirdLine.Text = "";
                    });
                }

                if (!DataValidator.ValidateData(song.Lyrics, song.CurrentLyricsRoll))
                    continue;

                LyricsRoll lyricsRoll = song.CurrentLyricsRoll;

                if (!DataValidator.ValidateData(lyricsRoll))
                    continue;

                await this.Dispatcher.InvokeAsync(() =>
                {
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
                });
            }
        }

        private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.firstLine.Text = "";
                this.secondLine.Text = "";
                this.thirdLine.Text = "";
                this.provider.Text = "";
                this.currentTitle.Text = "";
                this.provider.Text = "";
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (this._settings == null)
            {
                this._settings = new Settings();
                this._settings.Closed += (o, args) => this._settings = null;
                this._settings.Show();
            }
            else
            {
                this._settings.Show();
            }
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core.INSTANCE.DisposeEverything();
        }
    }
}

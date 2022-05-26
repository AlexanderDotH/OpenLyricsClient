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
using LyricsWPF.Backend;
using LyricsWPF.Backend.Collector;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Settings _settings;

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

            //BindText(currentLine, "Line");
            Core.INSTANCE.SongHandler.SongChanged += SongHandlerOnSongChanged;


            var t = new TaskFactory().StartNew(async () =>
            {
                while (!Core.IsDisposed())
                {
                    await Task.Delay(200);

                    Song song = Core.INSTANCE.SongHandler.CurrentSong;

                    if (DataValidator.ValidateData(song))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            this.currentTitle.Text = song.Title;
                            this.currentFullTitle.Text = song.Title;

                            this.timeFrom.Text = song.ProgressString;
                            this.timeTo.Text = song.MaxProgressString;
                        });

                        if (DataValidator.ValidateData(song.Lyrics) &&
                            DataValidator.ValidateData(song.Lyrics.LyricProvider))
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                this.provider.Text = "Powered by " + song.Lyrics.LyricProvider;
                                this.fullLyricText.Text = song.Lyrics.FullLyrics;
                            });
                        }

                        this.Dispatcher.Invoke(() =>
                        {
                            this.pgSongProgress.Value = song.GetPercentage();
                        });

                        if (DataValidator.ValidateData(song.State))
                        {
                            if (song.State == SongState.NO_LYRICS_AVAILABLE)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    this.firstLine.Text = "";
                                    this.secondLine.Text = "Lyrics not found";
                                    this.thirdLine.Text = "";
                                    this.provider.Text = "";
                                });
                            }
                            else if (DataValidator.ValidateData(song.Lyrics) &&
                                     DataValidator.ValidateData(song.CurrentLyricsRoll) &&
                                     song.State == SongState.HAS_LYRICS_AVAILABLE)
                            {
                                LyricsRoll lyricsRoll = song.CurrentLyricsRoll;

                                if (DataValidator.ValidateData(lyricsRoll))
                                {
                                    this.Dispatcher.Invoke(() =>
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
                        }
                    }
                }
            }, Core.INSTANCE.CancellationTokenSource.Token);

            //Thread trd = new Thread(t =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(10);

            //        Song song = core.SongHandler.CurrentSong;
            //        if (DataValidator.ValidateSong(song))
            //        {
            //            this.Dispatcher.Invoke((Action)(() =>
            //            {
            //                this.currentLine.Text = song.CurrentLyricPart.Part;
            //            }));
            //            this.Dispatcher.InvokeAsync(() =>
            //            {
            //                while (!Core.IsDisposed())
            //                {
            //                    this.currentLine.Text = song.CurrentLyricPart.Part;
            //                }
            //            }, DispatcherPriority.Normal);
            //        }

            //    }
            //});
            //trd.Start();
        }

        private void SongHandlerOnSongChanged(object sender, SongChangedEventArgs songchangedevent)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.firstLine.Text = "";
                this.secondLine.Text = "";
                this.thirdLine.Text = "";
                this.provider.Text = "";
            });
        }

        static void BindText(TextBlock textBox, string property)
        {
            DependencyProperty textProp = TextBlock.TextProperty;
            if (!BindingOperations.IsDataBound(textBox, textProp))
            {
                Binding b = new Binding(property);
                BindingOperations.SetBinding(textBox, textProp, b);
            }
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

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            //var col = new LyricCollector();
            //foreach (var s in col.CollectLyrics("Don´t You Worry Child", new []{ "Swedish House" }, "NetEase").LyricParts)
            //{
            //    Console.WriteLine(s.Part + " : " + s.Time);
            //}

            //var col = new LyricCollector();
            //foreach (var s in col.CollectLyrics(new SongRequestObject(Core.INSTANCE.SongHandler.CurrentSong.Title, Core.INSTANCE.SongHandler.CurrentSong.Artists), "NetEase").LyricParts)
            //{
            //    Console.WriteLine(s.Part + " : " + s.Time);
            //}
            //var _lyricCollector = new LyricCollector("Heros");
            //var _lyricDeserializer = new LyricDeserializer(_lyricCollector.CollectLyrics());
            //var _jsonFullLyrics = _lyricDeserializer.deserialize();

            //foreach (var l in _jsonFullLyrics)
            //{
            //    Console.WriteLine(l.lyrics);
            //}
            //SpotifyService spotifyService = new SpotifyService();
            //spotifyService.startAuthorization();
            //LyricHandler lyricHandler = new LyricHandler("b3d1e5357f438d523562d175cf697244");
            //lyricHandler.getLyric();
        }

        private void Window_Closing_1(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Core.INSTANCE.DisposeEverything();
        }
    }
}

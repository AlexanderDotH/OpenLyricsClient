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
using LyricsWPF.Backend.Deserializer;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.fullLyricText.Text = @"";
            Core core = new Core();

            //BindText(currentLine, "Line");


            var t = new TaskFactory().StartNew(() =>
            {
                while (!Core.IsDisposed())
                {
                    Thread.Sleep(250);

                    Song song = core.SongHandler.CurrentSong;

                    if (DataValidator.ValidateSong(song))
                    {
                        this.currentLine.Dispatcher.Invoke(() =>
                        {
                            this.currentLine.Text = song.CurrentLyricPart.Part;
                        }, DispatcherPriority.Normal);

                        this.currentTitle.Dispatcher.Invoke(() =>
                        {
                            this.currentTitle.Text = song.Title;
                        }, DispatcherPriority.Normal);
                    }
                }
            });

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
            Settings settings = new Settings();
            settings.Show();
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            //var col = new LyricCollector();
            //foreach (var s in col.CollectLyrics("Don´t You Worry Child", new []{ "Swedish House" }, "NetEase").LyricParts)
            //{
            //    Console.WriteLine(s.Part + " : " + s.Time);
            //}

            var col = new LyricCollector();
            foreach (var s in col.CollectLyrics(new SongRequestObject(Core.INSTANCE.SongHandler.CurrentSong.Title, Core.INSTANCE.SongHandler.CurrentSong.Artists), "NetEase").LyricParts)
            {
                Console.WriteLine(s.Part + " : " + s.Time);
            }
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

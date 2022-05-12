using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async;
using DevBase.Generic;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Events.EventHandler;
using LyricsWPF.Backend.Handler.Lyrics;
using LyricsWPF.Backend.Handler.Song.SongProvider;
using LyricsWPF.Backend.Handler.Song.SongProvider.Spotify;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Song
{
    class SongHandler : IHandler
    {
        private SongStageChange _songStageChange;

        private GenericList<Tuple<ISongProvider, EnumSongProvider>> _songProviders;
        private SongProviderChooser _songProviderChooser;

        private Task _manageCurrentSongTask;
        private Task _songInformationTask;
        private bool _disposed;

        private Debugger<SongHandler> _debugger;

        public event SongChangedEventHandler SongChanged;

        public SongHandler()
        {
            this._debugger = new Debugger<SongHandler>(this);

            this._songProviders = new GenericList<Tuple<ISongProvider, EnumSongProvider>>();
            this._songProviders.Add(new Tuple<ISongProvider, EnumSongProvider>(new SpotifySongProvider(), EnumSongProvider.SPOTIFY));

            this._songProviderChooser = new SongProviderChooser();

            this._songStageChange = new SongStageChange();

            this._manageCurrentSongTask = new Task(async() => await ManageCurrentSong(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
            this._manageCurrentSongTask.Start();

            if (Environment.GetCommandLineArgs().Contains("--enable-command-output"))
            {
                this._songInformationTask = new Task(async () => SongInformation(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning);
                this._songInformationTask.Start();
            }

            this._disposed = false;
        }

        private async Task ManageCurrentSong()
        {
            while (!this._disposed)
            {
                if (DataValidator.ValidateData(this._songStageChange) && 
                    DataValidator.ValidateData(this._songProviderChooser))
                {
                    if (this._songStageChange.HasSongChanged(GetCurrentSong()))
                    {
                        ISongProvider songProvider = GetSongProvider(this._songProviderChooser.GetSongProvider());
                        Song song = await songProvider.UpdateCurrentPlaybackTrack();

                        //                                      Idk why but it works
                        if (DataValidator.ValidateData(song) && this._songStageChange.HasSongChanged(song))
                        {
                            OnSongChanged(new SongChangedEventArgs(song));
                        }
                    }
                }
            }
        }

        private void SongInformation()
        {
            while (!this._disposed)
            {
                PrintSongState(GetCurrentSong());
            }
        }

        private void PrintSongState(Song song)
        {
            if (DataValidator.ValidateData(song) &&
                DataValidator.ValidateData(song.Title, song.Time))
            {
                this._debugger.Write("Title: " + song.Title, DebugType.INFO);
                this._debugger.Write("Time: " + song.Time, DebugType.INFO);

                if (DataValidator.ValidateData(song.CurrentLyricPart) &&
                    DataValidator.ValidateData(song.CurrentLyricPart.Part, song.CurrentLyricPart.Time))
                {
                    this._debugger.Write("LyricPart: " + song.CurrentLyricPart.Part, DebugType.INFO);
                }
            }
        }

        private ISongProvider GetSongProvider(EnumSongProvider enumSongProvider)
        {
            for (int i = 0; i < this._songProviders.Count; i++)
            {
                Tuple<ISongProvider, EnumSongProvider> item = this._songProviders[i];
                if (item.Item2.Equals(enumSongProvider))
                {
                    return item.Item1;
                }
            }

            return null;
        }

        private Song GetCurrentSong()
        {
            if (DataValidator.ValidateData(this._songProviderChooser))
            {
                ISongProvider songProvider = GetSongProvider(this._songProviderChooser.GetSongProvider());
                if (DataValidator.ValidateData(songProvider))
                {
                    return songProvider.GetCurrentSong();
                }
            }

            return null;
        }

        protected virtual void OnSongChanged(SongChangedEventArgs songChangedEventArgs)
        {
            SongChangedEventHandler songChangedEventHandler = SongChanged;
            songChangedEventHandler?.Invoke(this, songChangedEventArgs);
        }

        public Song CurrentSong
        {
            get => GetCurrentSong();
        }

        public void Dispose()
        {
            this._disposed = true;

            try
            {
                for (int i = 0; i < this._songProviders.Count; i++)
                {
                    this._songProviders[i].Item1.Dispose();
                }
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }
    }
}

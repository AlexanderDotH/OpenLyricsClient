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
    class NewSongHandler : IHandler
    {
        private Song _currentSong;
        private SongStageChange _songStageChange;

        private GenericList<Tuple<ISongProvider, EnumSongProvider>> _songProviders;
        private SongProviderChooser _songProviderChooser;

        private Thread _manageCurrentSongThread;
        private Thread _manageCurrentSongUpdateThread;
        private Thread _songInformationThread;
        private bool _disposed;

        private Debugger<NewSongHandler> _debugger;

        public event SongChangedEventHandler SongChanged;

        public NewSongHandler()
        {
            this._debugger = new Debugger<NewSongHandler>(this);

            this._songProviders = new GenericList<Tuple<ISongProvider, EnumSongProvider>>();
            this._songProviders.Add(new Tuple<ISongProvider, EnumSongProvider>(new SpotifySongProvider(this), EnumSongProvider.SPOTIFY));

            this._songProviderChooser = new SongProviderChooser();

            this._songStageChange = new SongStageChange();

            this._manageCurrentSongThread = new Thread(ManageCurrentSong);
            this._manageCurrentSongThread.Start();

            this._manageCurrentSongUpdateThread = new Thread(ManageCurrentSongUpdate);
            this._manageCurrentSongUpdateThread.Start();

            this._songInformationThread = new Thread(SongInformation);
            this._songInformationThread.Start();


            this._disposed = false;
        }

        private void ManageCurrentSong()
        {
            while (!this._disposed)
            {
                if (DataValidator.ValidateData(this._songStageChange))
                {
                    if (this._songStageChange.HasSongChanged(this._currentSong))
                    {
                        OnSongChanged(new SongChangedEventArgs());
                        this._debugger.Write("Song has been changed", DebugType.INFO);
                    }
                }
            }
        }

        private void ManageCurrentSongUpdate()
        {
            while (!this._disposed)
            {
                if (DataValidator.ValidateData(this._songProviderChooser))
                {
                    ISongProvider songProvider = GetSongProvider(this._songProviderChooser.GetSongProvider());
                    if (DataValidator.ValidateData(songProvider))
                    {
                        this._currentSong = songProvider.GetCurrentSong();
                    }
                }
            }
        }

        private void SongInformation()
        {
            while (!this._disposed)
            {
                PrintSongState(this._currentSong);
            }
        }

        private void PrintSongState(Song song)
        {
            if (DataValidator.ValidateData(song) &&
                DataValidator.ValidateData(song.Title, song.Time))
            {
                this._debugger.Write("Title: " + song.Title, DebugType.INFO);
                this._debugger.Write("Time: " + song.Time, DebugType.INFO);

                if (DataValidator.ValidateData(song.CurrentLyricPart))
                {
                    this._debugger.Write("LyricPart: " + song.CurrentLyricPart.Part, DebugType.INFO);
                }
            }
        }

        private ISongProvider GetSongProvider(EnumSongProvider enumSongProvider)
        {
            foreach (Tuple<ISongProvider, EnumSongProvider> item in _songProviders)
            {
                if (item.Item2.Equals(enumSongProvider))
                {
                    return item.Item1;
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
            get => _currentSong;
            set => _currentSong = value;
        }

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}

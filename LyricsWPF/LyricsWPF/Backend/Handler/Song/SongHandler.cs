using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async;
using DevBase.Async.Task;
using DevBase.Generic;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Events;
using LyricsWPF.Backend.Events.EventArgs;
using LyricsWPF.Backend.Events.EventHandler;
using LyricsWPF.Backend.Handler.Lyrics;
using LyricsWPF.Backend.Handler.Song.SongProvider;
using LyricsWPF.Backend.Handler.Song.SongProvider.Spotify;
using LyricsWPF.Backend.Handler.Song.SongProvider.Tidal;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Song
{
    class SongHandler : IHandler
    {
        private SongStageChange _songStageChange;

        private GenericTupleList<ISongProvider, EnumSongProvider> _songProviders;
        private SongProviderChooser _songProviderChooser;

        private bool _disposed;
        private TaskSuspensionToken _manageCurrentSongSuspensionToken;
        private TaskSuspensionToken _songInformationSuspensionToken;

        private Debugger<SongHandler> _debugger;

        public event SongChangedEventHandler SongChanged;

        public SongHandler()
        {
            this._debugger = new Debugger<SongHandler>(this);

            this._songProviders = new GenericTupleList<ISongProvider, EnumSongProvider>();
            this._songProviders.Add(new Tuple<ISongProvider, EnumSongProvider>(new SpotifySongProvider(), EnumSongProvider.SPOTIFY));
            this._songProviders.Add(new Tuple<ISongProvider, EnumSongProvider>(new TidalSongProvider(), EnumSongProvider.TIDAL));

            this._songProviderChooser = new SongProviderChooser();

            this._songStageChange = new SongStageChange();

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _manageCurrentSongSuspensionToken,
                new Task(async () => await ManageCurrentSong(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.SONGHANDLER_MANAGECURRENTSONG);

            if (EnvironmentUtils.IsDebugLogEnabled())
            {
                Core.INSTANCE.TaskRegister.RegisterTask(
                    out _songInformationSuspensionToken,
                    new Task(async () => await SongInformation(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning),
                    EnumRegisterTypes.SONGHANDLER_SONGINFORMATION);
            }

            this._disposed = false;
        }

        private async Task ManageCurrentSong()
        {
            while (!this._disposed)
            {
                await this._manageCurrentSongSuspensionToken.WaitForRelease();

                await Task.Delay(100);

                if (DataValidator.ValidateData(this._songStageChange) && 
                    DataValidator.ValidateData(this._songProviderChooser))
                {
                    Song currentSong = GetCurrentSong();

                    if (this._songStageChange.HasSongChanged(currentSong))
                    {
                        BeforeSongChanged(new SongChangedEventArgs(currentSong, EventType.PRE));

                        ISongProvider songProvider = GetSongProvider(this._songProviderChooser.GetSongProvider());

                        if (!DataValidator.ValidateData(songProvider))
                            continue;

                        Song song = await songProvider.UpdateCurrentPlaybackTrack();

                        //                                      Idk why but it works
                        if (DataValidator.ValidateData(song) && this._songStageChange.HasSongChanged(song))
                        {
                            AfterSongChanged(new SongChangedEventArgs(song, EventType.POST));
                        }
                    }
                }
            }
        }

        private async Task SongInformation()
        {
            while (!this._disposed)
            {
                await this._songInformationSuspensionToken.WaitForRelease();

                await Task.Delay(10);
                PrintSongState(GetCurrentSong());
            }
        }

        private void PrintSongState(Song song)
        {
            if (DataValidator.ValidateData(song) &&
                DataValidator.ValidateData(song.Title, song.Time, song.TimeThreshold))
            {
                this._debugger.Write("Name: " + song.Title, DebugType.INFO);
                this._debugger.Write("Time: " + song.Time, DebugType.INFO);
                this._debugger.Write("Threshold: " + song.TimeThreshold, DebugType.INFO);


                if (DataValidator.ValidateData(song.CurrentLyricPart) &&
                    DataValidator.ValidateData(song.CurrentLyricPart.Part, song.CurrentLyricPart.Time))
                {
                    this._debugger.Write("LyricPart: " + song.CurrentLyricPart.Part, DebugType.INFO);
                }
            }
        }

        public ISongProvider GetSongProvider(EnumSongProvider enumSongProvider)
        {
            return this._songProviders.FindEntry(enumSongProvider);
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

        public void RequestNewSong()
        {
            BeforeSongChanged(new SongChangedEventArgs(null, EventType.PRE));
        }

        protected virtual void AfterSongChanged(SongChangedEventArgs songChangedEventArgs)
        {
            SongChangedEventHandler songChangedEventHandler = SongChanged;
            songChangedEventHandler?.Invoke(this, songChangedEventArgs);
        }

        protected virtual void BeforeSongChanged(SongChangedEventArgs songChangedEventArgs)
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

            this._songProviderChooser.Dispose();

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.SONGHANDLER_MANAGECURRENTSONG, EnumRegisterTypes.SONGHANDLER_SONGINFORMATION);

            try
            {
                for (int i = 0; i < this._songProviders.Length; i++)
                {
                    this._songProviders.Get(i).Item1.Dispose();
                }
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }
    }
}

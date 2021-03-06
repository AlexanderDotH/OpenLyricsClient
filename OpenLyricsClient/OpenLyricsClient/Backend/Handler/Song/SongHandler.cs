using System;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Handler.Song.SongProvider;
using OpenLyricsClient.Backend.Handler.Song.SongProvider.Spotify;
using OpenLyricsClient.Backend.Handler.Song.SongProvider.Tidal;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Song
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

                await Task.Delay(50);

                if (DataValidator.ValidateData(this._songStageChange) && 
                    DataValidator.ValidateData(this._songProviderChooser))
                {
                    Structure.Song.Song currentSong = GetCurrentSong();
                    
                    //POST WIRD NICHT IMMER AUSGEFÜHRT
                    if (this._songStageChange.HasSongChanged(currentSong))
                    {
                        SongChangedEvent(new SongChangedEventArgs(currentSong, EventType.PRE));

                        //
                        ISongProvider songProvider = GetSongProvider(this._songProviderChooser.GetSongProvider());
                        if (!DataValidator.ValidateData(songProvider))
                            continue;

                        Structure.Song.Song song = await songProvider.UpdateCurrentPlaybackTrack();
                        //

                        SongChangedEvent(new SongChangedEventArgs(song, EventType.POST));
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

        private void PrintSongState(Structure.Song.Song song)
        {
            if (DataValidator.ValidateData(song) &&
                DataValidator.ValidateData(song.Title, song.Time, song.TimeThreshold))
            {
                this._debugger.Write("Name: " + song.Title, DebugType.INFO);
                this._debugger.Write("----------------------", DebugType.INFO);
                this._debugger.Write("Time in sec: " + song.Time / 1000, DebugType.INFO);
                this._debugger.Write("Time in ms: " + song.Time, DebugType.INFO);
                this._debugger.Write("----------------------", DebugType.INFO);
                this._debugger.Write("Progress in sec: " + song.ProgressMs / 1000, DebugType.INFO);
                this._debugger.Write("Progress in ms: " + song.ProgressMs, DebugType.INFO);
                this._debugger.Write("----------------------", DebugType.INFO);
                this._debugger.Write("Diff in ms: " + Math.Abs(song.ProgressMs - song.Time), DebugType.INFO);
                this._debugger.Write("----------------------", DebugType.INFO);

                this._debugger.Write("Threshold: " + song.TimeThreshold, DebugType.INFO);
                this._debugger.Write("----------------------", DebugType.INFO);

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

        private Structure.Song.Song GetCurrentSong()
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
            this._songStageChange.Reset();
            SongChangedEvent(new SongChangedEventArgs(null, EventType.PRE));
        }

        protected virtual void SongChangedEvent(SongChangedEventArgs songChangedEventArgs)
        {
            SongChangedEventHandler songChangedEventHandler = SongChanged;
            songChangedEventHandler?.Invoke(this, songChangedEventArgs);
        }
        
        public Structure.Song.Song CurrentSong
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

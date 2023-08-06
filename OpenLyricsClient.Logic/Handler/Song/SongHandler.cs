using Avalonia.Threading;
using DevBase.Async.Task;
using DevBase.Generics;
using OpenLyricsClient.Logic.Collector.Song;
using OpenLyricsClient.Logic.Debugger;
using OpenLyricsClient.Logic.Events;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Events.EventHandler;
using OpenLyricsClient.Logic.Handler.Artwork;
using OpenLyricsClient.Logic.Handler.Lyrics;
using OpenLyricsClient.Logic.Handler.Song.SongProvider;
using OpenLyricsClient.Logic.Handler.Song.SongProvider.Spotify;
using OpenLyricsClient.Logic.Helper;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Handler.Song
{
    public class SongHandler : IHandler
    {
        private SongStageChange _songStageChange;

        private ATupleList<ISongProvider, EnumSongProvider> _songProviders;
        private SongProviderChooser _songProviderChooser;

        private bool _disposed;
        private TaskSuspensionToken _manageCurrentSongSuspensionToken;
        private TaskSuspensionToken _songInformationSuspensionToken;

        private Debugger<SongHandler> _debugger;

        private SongCollector _songCollector;
        
        public event SongChangedEventHandler SongChanged;
        public event SongUpdatedEventHandler SongUpdated;

        public SongHandler()
        {
            this._debugger = new Debugger<SongHandler>(this);

            this._songProviders = new ATupleList<ISongProvider, EnumSongProvider>();
            this._songProviders.Add(new Tuple<ISongProvider, EnumSongProvider>(new SpotifySongProvider(), EnumSongProvider.SPOTIFY));
            //this._songProviders.Add(new Tuple<ISongProvider, EnumSongProvider>(new TidalSongProvider(), EnumSongProvider.TIDAL));

            this._songProviderChooser = new SongProviderChooser();
            this._songStageChange = new SongStageChange();

            Core.INSTANCE.TaskRegister.Register(
                out _manageCurrentSongSuspensionToken,
                new Task(async () => await ManageCurrentSong(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.SONGHANDLER_MANAGECURRENTSONG);

            this.SongChanged += OnSongChanged;
            
            this._disposed = false;
        }

        private void OnSongChanged(object sender, SongChangedEventArgs songChangedEventArgs)
        {
            if (songChangedEventArgs.EventType == EventType.PRE)
                return;
            
            this._debugger.Write("Song collection fired", DebugType.INFO);
            
            Task.Factory.StartNew(async () =>
            {
                await this._songCollector.FireSongCollector(songChangedEventArgs);
            });
        }

        public void InitializeSongCollector(LyricHandler lyricHandler, ArtworkHandler artworkHandler)
        {
            this._songCollector = new SongCollector(this, lyricHandler, artworkHandler);
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
                    Shared.Structure.Song.Song currentSong = GetCurrentSong();
                    
                    //POST WIRD NICHT IMMER AUSGEFÜHRT
                    if (this._songStageChange.HasSongChanged(currentSong))
                    {
                        SongChangedEvent(new SongChangedEventArgs(currentSong, EventType.PRE));

                        //
                        ISongProvider songProvider = GetSongProvider(this._songProviderChooser.GetSongProvider());
                        if (!DataValidator.ValidateData(songProvider))
                            continue;

                        Shared.Structure.Song.Song song = await songProvider.UpdateCurrentPlaybackTrack();
                        Core.INSTANCE.CacheManager.WriteToCache(SongRequestObject.FromSong(song));
                        //

                        SongChangedEvent(new SongChangedEventArgs(song, EventType.POST));
                        
                        MemoryHelper.ForceGC();
                    }
                }
            }
        }

        public ISongProvider GetSongProvider(EnumSongProvider enumSongProvider)
        {
            return this._songProviders.FindEntry(enumSongProvider);
        }

        public EnumSongProvider SongProvider
        {
            get => this._songProviderChooser.GetSongProvider();
        }

        private Shared.Structure.Song.Song GetCurrentSong()
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
        
        public async Task SongUpdatedEvent()
        {
            SongUpdatedEventHandler songUpdatedEvent = SongUpdated;
            songUpdatedEvent?.Invoke(this);
        }
        
        public Shared.Structure.Song.Song CurrentSong
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

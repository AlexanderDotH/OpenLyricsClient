using System;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBase.Typography;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Services.Services;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider
{
    class SongProviderChooser
    {
        private EnumSongProvider _currentSongProvider;

        private TaskSuspensionToken _taskSuspensionToken;
        private bool _disposed;

        private Debugger<SongProviderChooser> _debugger;

        private readonly object[] _spotifyTypes = new object[]
        {
            EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATEPLAYBACK,
            EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATESONGDATA,
            EnumRegisterTypes.SPOTIFYSONGPROVIDER_TIMESYNC,
            EnumRegisterTypes.SPOTIFY_REFRESHTOKEN
        };

        private readonly object[] _tidalTypes = new object[]
        {
            EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN,
            EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK,
            EnumRegisterTypes.TIDALSONGPROVIDER_UPDATETIME,
            EnumRegisterTypes.TIDAL_REFRESHTOKEN,
            EnumRegisterTypes.TIDALPROGRESSLISTENER_FINDADDRESS
        };

        public SongProviderChooser()
        {
            this._debugger = new Debugger<SongProviderChooser>(this);
            this._currentSongProvider = EnumSongProvider.NONE;

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _taskSuspensionToken, 
                new Task(async () => await SongProviderChooserTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.SONG_PROVIDER_CHOOSER);
        }

        private async Task SongProviderChooserTask()
        {
            while (!this._disposed)
            {
                await this._taskSuspensionToken.WaitForRelease();

                if (!DataValidator.ValidateData(Core.INSTANCE.WindowLogger))
                    continue;

                EnumSongProvider songProvider = EnumSongProvider.NONE;

                IService spotifyService = Core.INSTANCE.ServiceHandler.GetServiceByName("Spotify");
                IService tidalService = Core.INSTANCE.ServiceHandler.GetServiceByName("Tidal");

                string spotifyProcess = spotifyService.ProcessName();
                string tidalProcess = tidalService.ProcessName();

                GenericList<string> lastWindows = Core.INSTANCE.WindowLogger.LastWindows(spotifyProcess, tidalProcess);
                GenericList<string> foundProcesses = ProcessUtils.GetRunningProcesses(tidalProcess, spotifyProcess);

                if (!foundProcesses.IsEmpty())
                {
                    if (foundProcesses.Get(0).Equals(spotifyProcess))
                        songProvider = EnumSongProvider.SPOTIFY;

                    if (foundProcesses.Get(0).Equals(tidalProcess))
                        songProvider = EnumSongProvider.TIDAL;
                }

                if (!lastWindows.IsEmpty())
                {
                    if (spotifyService.IsConnected() && lastWindows.Get(0).Equals(spotifyProcess))
                        songProvider = EnumSongProvider.SPOTIFY;

                    if (tidalService.IsConnected() && lastWindows.Get(0).Equals(tidalProcess))
                        songProvider = EnumSongProvider.TIDAL;
                }

                if (songProvider == EnumSongProvider.NONE)
                    continue;

                if (!songProvider.Equals(this._currentSongProvider))
                {
                    this._debugger.Write("SongProvider has been changed to: " + new AString(songProvider.ToString()).CapitalizeFirst(), DebugType.INFO);
                    this._currentSongProvider = songProvider;

                    if (songProvider == EnumSongProvider.SPOTIFY)
                    {
                        ResumeProvider(EnumSongProvider.SPOTIFY);
                        SuspendProvider(EnumSongProvider.TIDAL);
                    }
                    else if (songProvider == EnumSongProvider.TIDAL)
                    {
                        ResumeProvider(EnumSongProvider.TIDAL);
                        SuspendProvider(EnumSongProvider.SPOTIFY);
                    }
                }

                await Task.Delay(1000);

            }
        }

        private void ResumeProvider(EnumSongProvider songProvider)
        {
            switch (songProvider)
            {
                case EnumSongProvider.SPOTIFY:
                {
                    Core.INSTANCE.TaskRegister.ResumeByArray(this._spotifyTypes);
                    break;
                }
                case EnumSongProvider.TIDAL:
                {
                    Core.INSTANCE.TaskRegister.ResumeByArray(this._tidalTypes);
                    break;
                }
            }
        }

        private void SuspendProvider(EnumSongProvider songProvider)
        {
            switch (songProvider)
            {
                case EnumSongProvider.SPOTIFY:
                {
                    Core.INSTANCE.TaskRegister.SuspendByArray(this._spotifyTypes);
                    break;
                }
                case EnumSongProvider.TIDAL:
                {
                    Core.INSTANCE.TaskRegister.SuspendByArray(this._tidalTypes);
                    break;
                }
            }
        }

        public EnumSongProvider GetSongProvider()
        {
            return this._currentSongProvider;
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.SONG_PROVIDER_CHOOSER);
        }
    }
}

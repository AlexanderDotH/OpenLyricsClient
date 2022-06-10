using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBase.Typography;
using DevBase.Utilities;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Song.SongProvider
{
    class SongProviderChooser
    {
        private EnumSongProvider _currentSongProvider;

        private TaskSuspensionToken _taskSuspensionToken;
        private bool _disposed;

        private Debugger<SongProviderChooser> _debugger;

        public SongProviderChooser()
        {
            this._debugger = new Debugger<SongProviderChooser>(this);

            this._currentSongProvider = EnumSongProvider.NONE;

            Core.INSTANCE.TaskRegister.Suspend(EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATEPLAYBACK, EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATESONGDATA, EnumRegisterTypes.SPOTIFYSONGPROVIDER_TIMESYNC,
                EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN, EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK, EnumRegisterTypes.TIDALSONGPROVIDER_UPDATETIME);

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
                await Task.Delay(100);

                if (!DataValidator.ValidateData(Core.INSTANCE.WindowLogger))
                    continue;

                EnumSongProvider songProvider;

                if (Core.INSTANCE.WindowLogger.IsLastWindow("Spotify"))
                {
                    songProvider = EnumSongProvider.SPOTIFY;
                } 
                else if (Core.INSTANCE.WindowLogger.IsLastWindow("TIDAL"))
                {
                    songProvider = EnumSongProvider.TIDAL;
                }
                else
                {
                    songProvider = EnumSongProvider.NONE;
                }

                if (songProvider != EnumSongProvider.NONE && 
                    !songProvider.Equals(this._currentSongProvider))
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
            }
        }

        private void ResumeProvider(EnumSongProvider songProvider)
        {
            switch (songProvider)
            {
                case EnumSongProvider.SPOTIFY:
                {
                    Core.INSTANCE.TaskRegister.Resume(
                        EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATEPLAYBACK,
                        EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATESONGDATA,
                        EnumRegisterTypes.SPOTIFYSONGPROVIDER_TIMESYNC);
                    break;
                }
                case EnumSongProvider.TIDAL:
                {
                    Core.INSTANCE.TaskRegister.Resume(
                        EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN,
                        EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK,
                        EnumRegisterTypes.TIDALSONGPROVIDER_UPDATETIME);
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
                    Core.INSTANCE.TaskRegister.Suspend(
                        EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATEPLAYBACK,
                        EnumRegisterTypes.SPOTIFYSONGPROVIDER_UPDATESONGDATA,
                        EnumRegisterTypes.SPOTIFYSONGPROVIDER_TIMESYNC);
                    break;
                }
                case EnumSongProvider.TIDAL:
                {
                    Core.INSTANCE.TaskRegister.Suspend(
                        EnumRegisterTypes.TIDALSONGPROVIDER_LOGIN,
                        EnumRegisterTypes.TIDALSONGPROVIDER_UPDATECURRENTTRACK,
                        EnumRegisterTypes.TIDALSONGPROVIDER_UPDATETIME);
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

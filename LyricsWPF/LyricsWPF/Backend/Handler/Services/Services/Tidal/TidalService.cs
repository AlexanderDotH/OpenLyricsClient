using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.IO;
using DevBase.Typography;
using DevBase.Utilities;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Json;
using LyricsWPF.Backend.Utils;
using LyricsWPF.Backend.Utils.Service;
using Newtonsoft.Json;
using TidalLib;

namespace LyricsWPF.Backend.Handler.Services.Services.Tidal
{
    internal class TidalService : IService
    {

        private bool _disposed;

        private Debugger<TidalService> _debugger;

        private Task _refeshTokenTask;

        private TidalAccess _tidalAccess;
        private Task _loginTask;

        public TidalService()
        {
            this._debugger = new Debugger<TidalService>(this);

            this._disposed = false;

            this._refeshTokenTask = new Task(async () => await RefreshToken(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.None);
            this._refeshTokenTask.Start();
        }
        
        private async Task RefreshToken()
        {
            while (!this._disposed)
            {
                await Task.Delay(2000);

                if (Core.INSTANCE.SettingManager.Settings.TidalAccess.IsTidalConnected)
                {
                    if (DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.TidalAccess))
                    {
                        long savedTime = Core.INSTANCE.SettingManager.Settings.TidalAccess.ExpirationDate;
                        long currentTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                        if (currentTime > savedTime)
                        {

                            JsonTidalAccountRefreshAccess refresh =
                                await TidalUtils.RefreshToken(Core.INSTANCE.SettingManager.Settings.TidalAccess);

                            if (!DataValidator.ValidateData(refresh))
                                continue;


                            Core.INSTANCE.SettingManager.Settings.TidalAccess.AccessToken = refresh.AccessToken;
                            Core.INSTANCE.SettingManager.Settings.TidalAccess.ExpirationDate = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(refresh.ExpiresIn))
                                .ToUnixTimeMilliseconds(); ;
                            Core.INSTANCE.SettingManager.WriteSettings();

                            this._debugger.Write("Refreshed Tidal!", DebugType.INFO);
                        }
                    }
                }
            }
        }
        
        public string ServiceName()
        {
            return "Tidal";
        }

        public async Task StartAuthorization()
        {

            JsonTidalAuthDevice authDevice = await TidalUtils.RegisterDevice();

            Process.Start("https://" + authDevice.VerificationUriComplete);

            DateTimeOffset expires = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(authDevice.ExpiresIn));
            bool authorized = false;

            while (!authorized)
            {
                await Task.Delay(authDevice.Interval * 1000);

                if (DateTimeOffset.Now > expires)
                {
                    //Give user some feedback
                    return;
                }

                JsonTidalAccountAccess accountAccess = await TidalUtils.GetTokenFrom(authDevice);

                if (accountAccess == null)
                    continue;

                TidalAccess access = new TidalAccess();
                access.AccessToken = accountAccess.AccessToken;
                access.RefreshToken = accountAccess.RefreshToken;
                access.ExpirationDate = DateTimeOffset.Now.Add(TimeSpan.FromSeconds(accountAccess.ExpiresIn))
                    .ToUnixTimeMilliseconds();
                access.IsTidalConnected = true;
                access.UserID = accountAccess.User.UserId;

                Core.INSTANCE.SettingManager.Settings.TidalAccess = access;
                Core.INSTANCE.SettingManager.WriteSettings();

                authorized = true;
            }
        }

        public string GetAccessToken()
        {
            return Core.INSTANCE.SettingManager.Settings.TidalAccess.AccessToken;
        }

        public bool IsConnected()
        {
            return Core.INSTANCE.SettingManager.Settings.TidalAccess != null && Core.INSTANCE.SettingManager.Settings.TidalAccess.IsTidalConnected;
        }

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}

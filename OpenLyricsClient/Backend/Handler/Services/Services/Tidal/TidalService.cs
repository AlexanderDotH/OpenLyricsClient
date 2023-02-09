using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DevBase.Api.Apis.Tidal;
using DevBase.Api.Apis.Tidal.Structure.Json;
using DevBase.Async.Task;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Services.Services.Tidal
{
    public class TidalService : IService
    {

        private bool _disposed;

        private Debugger<TidalService> _debugger;

        private TaskSuspensionToken _refreshTokenSuspensionToken;

        private TidalAccess _tidalAccess;
        private TidalClient _tidalClient;

        public TidalService()
        {
            this._debugger = new Debugger<TidalService>(this);

            this._disposed = false;

            this._tidalClient = new TidalClient();

            Core.INSTANCE.TaskRegister.Register(
                out _refreshTokenSuspensionToken,
                new Task(async () => await RefreshToken(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.None), 
                EnumRegisterTypes.TIDAL_REFRESHTOKEN);
        }
        
        private async Task RefreshToken()
        {
            while (!this._disposed)
            {
                await this._refreshTokenSuspensionToken.WaitForRelease();
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
                                await this._tidalClient.RefreshToken(Core.INSTANCE.SettingManager.Settings.TidalAccess.RefreshToken);

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

        public async Task StartAuthorization()
        {
            JsonTidalAuthDevice authDevice = await this._tidalClient.RegisterDevice();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("https://" + authDevice.VerificationUriComplete);
                processStartInfo.UseShellExecute = true;
                Process.Start(processStartInfo);
            }

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

                JsonTidalAccountAccess accountAccess = await this._tidalClient.GetTokenFrom(authDevice);

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

        public async Task<bool> TestConnection()
        {
            if (!IsConnected())
                return false;

            JsonTidalSession session =
                await this._tidalClient.Login(Core.INSTANCE.SettingManager.Settings.TidalAccess.AccessToken);

            if (!DataValidator.ValidateData(session))
                return false;

            JsonTidalSearchResult result = await this._tidalClient.Search(session, "Never gonna give you up");

            return DataValidator.ValidateData(result);
        }

        public string ServiceName()
        {
            return "Tidal";
        }

        public string ProcessName()
        {
            return "TIDAL";
        }

        public TidalClient TidalClient
        {
            get => _tidalClient;
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.TIDAL_REFRESHTOKEN);
        }
    }
}

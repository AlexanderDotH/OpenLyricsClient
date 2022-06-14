using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Structure.Enum;
using Microsoft.Extensions.Configuration;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Handler.Services.Services.Spotify
{
    class SpotifyService : IService
    {
        private UserAccountsService _userAccountsService;
        private ConfigurationManager _configurationManager;

        private TaskSuspensionToken _refreshTokenSuspensionToken;

        private Debugger<SpotifyService> _debugger;
        private bool _disposed;

        public SpotifyService()
        {
            this._debugger = new Debugger<SpotifyService>(this);
            this._disposed = false;

            ConfigurationManager configurationManager = new ConfigurationManager();
            configurationManager["SpotifyApiClientId"] = "5506575c84334b25978bda35ee43e6fd";
            configurationManager["SpotifyApiClientSecret"] = "0896c04d78374ff2ad51a2c4f4c857ed";
            configurationManager["SpotifyAuthRedirectUri"] = "http://localhost:8080/callback";
            this._configurationManager = configurationManager;

            HttpClient httpClient = new HttpClient();
            this._userAccountsService = new UserAccountsService(httpClient, _configurationManager);

            Core.INSTANCE.TaskRegister.RegisterTask(
                out _refreshTokenSuspensionToken,
                new Task(async () => await RefreshToken(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.None),
                EnumRegisterTypes.SPOTIFY_REFRESHTOKEN);
        }

        private async Task RefreshToken()
        {
            while (!this._disposed)
            {
                await this._refreshTokenSuspensionToken.WaitForRelease();
                await Task.Delay(1000);

                if (Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected)
                {
                    if (Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess != null)
                    {
                        DateTime expire = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime;

                        if (DateTime.Now > expire)
                        {
                            await RefreshTokenRequest();
                            this._debugger.Write("Refreshed Spotify Token", DebugType.DEBUG);
                        }
                    }
                }
            }
        }

        public bool IsConnected()
        {
            return Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected && Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess != null;
        }

        public string GetAccessToken()
        {
            Task.Factory.StartNew(async() =>
            {
                await RefreshTokenRequest();

            }).Wait(Core.INSTANCE.CancellationTokenSource.Token);
            return Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess.AccessToken;
        }

        private async Task RefreshTokenRequest()
        {
            try
            {
                BearerAccessToken bearerAccess = await _userAccountsService.RefreshUserAccessToken(Core.INSTANCE.SettingManager.Settings.SpotifyAccess.RefreshToken);
                Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess = bearerAccess;

                DateTime bearerTime =
                    Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess.Expires.Value;

                DateTime settingTime = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime;

                if (bearerTime < settingTime)
                {
                    Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime =
                        settingTime.Add(TimeSpan.FromSeconds(bearerAccess.ExpiresIn));
                }
                else
                {
                    Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime = bearerAccess.Expires.Value.Add(TimeSpan.FromSeconds(bearerAccess.ExpiresIn));
                }

                Core.INSTANCE.SettingManager.WriteSettings();
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }

        public async Task StartAuthorization()
        {
            string state = Guid.NewGuid().ToString("N");

            GenericList<string> scopes = new GenericList<string>();
            scopes.Add("playlist-read-private");
            scopes.Add("playlist-read-collaborative");
            scopes.Add("streaming");
            scopes.Add("user-follow-read");
            scopes.Add("user-library-read");
            scopes.Add("user-read-private");
            scopes.Add("user-read-playback-state");
            scopes.Add("user-modify-playback-state");
            scopes.Add("user-read-currently-playing");
            scopes.Add("user-read-recently-played");

            string url = _userAccountsService.AuthorizeUrl(state, scopes.GetAsArray());

            Process.Start(url);

            Listener.Listener l = new Listener.Listener("http://localhost:8080/", "/callback", "code");

            string token = string.Empty;
            bool running = false;
            while (!l.Finished && !running)
            {
                if (l.Response != null)
                {
                    token = l.Response;
                    running = true;
                }
            }

            BearerAccessRefreshToken bearerAccessRefresh = await _userAccountsService.RequestAccessRefreshToken(token);

            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.RefreshToken = bearerAccessRefresh.RefreshToken;
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess = bearerAccessRefresh;
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected = true;
            Core.INSTANCE.SettingManager.WriteSettings();
        }

        string IService.ServiceName()
        {
            return "Spotify";
        }

        public string ProcessName()
        {
            return "Spotify";
        }

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.SPOTIFY_REFRESHTOKEN);
        }
    }
}

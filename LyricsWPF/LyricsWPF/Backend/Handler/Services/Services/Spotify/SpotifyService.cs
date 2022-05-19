using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Debug;
using Microsoft.Extensions.Configuration;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Handler.Services.Services.Spotify
{
    class SpotifyService : IService
    {
        private UserAccountsService _userAccountsService;
        private ConfigurationManager _configurationManager;

        private Task _refeshTokenTask;

        private Debugger<SpotifyService> _debugger;
        private bool _disposed;

        public SpotifyService()
        {
            this._debugger = new Debugger<SpotifyService>(this);
            this._disposed = false;

            ConfigurationManager configurationManager = new ConfigurationManager();
            configurationManager["SpotifyApiClientId"] = "d77eb56257c44acd8e1a123eabd2390c";
            configurationManager["SpotifyApiClientSecret"] = "48f5fc414d264247b87502daa0c68668";
            configurationManager["SpotifyAuthRedirectUri"] = "http://localhost:8080/callback";
            this._configurationManager = configurationManager;

            HttpClient httpClient = new HttpClient();
            this._userAccountsService = new UserAccountsService(httpClient, _configurationManager);

            this._refeshTokenTask = new Task(async () => await RefreshToken(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.None);
            this._refeshTokenTask.Start();
        }

        private async Task RefreshToken()
        {
            while (!this._disposed)
            {
                await Task.Delay(1000);

                if (Core.INSTANCE.Settings.IsSpotifyConnected)
                {
                    if (Core.INSTANCE.Settings.BearerAccess != null)
                    {
                        DateTime expire = Core.INSTANCE.Settings.SpotifyExpireTime.Value;
                        DateTime expiresTime = expire.AddSeconds(Core.INSTANCE.Settings.BearerAccess.ExpiresIn);

                        if (DateTime.Now > expiresTime)
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
            return Core.INSTANCE.Settings.IsSpotifyConnected && Core.INSTANCE.Settings.BearerAccess.AccessToken != null;
        }

        public string GetAccessToken()
        {
            Task.Factory.StartNew(async() =>
            {
                await RefreshTokenRequest();

            }).Wait(Core.INSTANCE.CancellationTokenSource.Token);
            return Core.INSTANCE.Settings.BearerAccess.AccessToken;
        }

        private async Task RefreshTokenRequest()
        {
            try
            {
                BearerAccessToken bearerAccess = await _userAccountsService.RefreshUserAccessToken(Core.INSTANCE.Settings.BearerAccess.RefreshToken);
                Core.INSTANCE.Settings.SpotifyExpireTime = bearerAccess.Expires;
                Core.INSTANCE.Settings.BearerAccess.AccessToken = bearerAccess.AccessToken;
                Core.INSTANCE.Settings.SpotifyExpireTime = bearerAccess.Expires.Value;
                Core.INSTANCE.Settings.BearerAccess.ExpiresIn = bearerAccess.ExpiresIn;
                Core.INSTANCE.WriteSettings();
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }

        public async Task StartAuthorization()
        {
            // TODO: Remove unnecessary permissions
            string state = Guid.NewGuid().ToString("N");
            string url = _userAccountsService.AuthorizeUrl(state,
                new[]
                {
                    "playlist-read-private,playlist-read-collaborative,streaming,user-follow-read,user-library-read,user-read-private,user-read-playback-state,user-modify-playback-state,user-read-currently-playing,user-read-recently-played"
                });

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

            Core.INSTANCE.Settings.BearerAccess = bearerAccessRefresh;
            Core.INSTANCE.Settings.SpotifyExpireTime = bearerAccessRefresh.Expires;
            Core.INSTANCE.Settings.IsSpotifyConnected = true;
            Core.INSTANCE.WriteSettings();
        }

        string IService.ServiceName()
        {
            return "Spotify";
        }

        public void Dispose()
        {
            this._disposed = true;
        }
    }
}

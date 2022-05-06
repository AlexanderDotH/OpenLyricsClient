using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SpotifyApi.NetCore.Authorization;

namespace LyricsWPF.Backend.Handler.Services.Services.Spotify
{
    class SpotifyService : IService
    {
        private UserAccountsService _userAccountsService;
        private ConfigurationManager _configurationManager;

        public SpotifyService()
        {
            ConfigurationManager configurationManager = new ConfigurationManager();
            configurationManager["SpotifyApiClientId"] = "d77eb56257c44acd8e1a123eabd2390c";
            configurationManager["SpotifyApiClientSecret"] = "48f5fc414d264247b87502daa0c68668";
            configurationManager["SpotifyAuthRedirectUri"] = "http://localhost:8080/callback";
            this._configurationManager = configurationManager;

            HttpClient httpClient = new HttpClient();
            this._userAccountsService = new UserAccountsService(httpClient, _configurationManager);
        }

        public bool IsConnected()
        {
            return Core.INSTANCE.Settings.IsSpotifyConnected && Core.INSTANCE.Settings.BearerAccess.AccessToken != null;
        }

        public async Task RefreshToken()
        {
            BearerAccessToken bearerAccess = await _userAccountsService.RefreshUserAccessToken(Core.INSTANCE.Settings.BearerAccess.RefreshToken);
            Core.INSTANCE.Settings.SpotifyExpireTime = bearerAccess.Expires;
            Core.INSTANCE.Settings.BearerAccess.AccessToken = bearerAccess.AccessToken;
            Core.INSTANCE.WriteSettings();
        }

        public string GetAccessToken()
        {
            RefreshToken();
            return Core.INSTANCE.Settings.BearerAccess.AccessToken;
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
    }
}

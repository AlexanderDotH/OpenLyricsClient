using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Markup;
using Avalonia.Controls;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.EnvFormat;
using DevBaseFormat.Structure;
using Microsoft.Extensions.Configuration;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Environment;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.External.CefNet.View;
using OpenLyricsClient.Frontend.View.Windows;
using SpotifyApi.NetCore;
using SpotifyApi.NetCore.Authorization;

namespace OpenLyricsClient.Backend.Handler.Services.Services.Spotify
{
    class SpotifyService : IService
    {
        private UserAccountsService _userAccountsService;
        private IConfiguration _configurationManager;

        private string _baseAuthUrl;
        private string _redirectUrl;
        
        private TaskSuspensionToken _refreshTokenSuspensionToken;

        private Debugger<SpotifyService> _debugger;
        private bool _disposed;

        public SpotifyService()
        {
            this._debugger = new Debugger<SpotifyService>(this);
            this._disposed = false;

            this._baseAuthUrl = "https://www.openlyricsclient.com/connect/spotify/begin";
            this._redirectUrl = "https://www.openlyricsclient.com/connect/spotify/complete"; 
            
            //Please dont steal me
            Dictionary<string, string> configurationManager = new Dictionary<string, string>();
            configurationManager["SpotifyApiClientId"] = "5506575c84334b25978bda35ee43e6fd";
            configurationManager["SpotifyApiClientSecret"] = "notset";
            configurationManager["SpotifyAuthRedirectUri"] = this._redirectUrl;

            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(configurationManager);

            this._configurationManager = builder.Build();

            HttpClient httpClient = new HttpClient();
            this._userAccountsService = new AccountsService(httpClient, _configurationManager);

            Core.INSTANCE.TaskRegister.Register(
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

        public async Task<bool> TestConnection()
        {
            if (!IsConnected())
                return false;

            if (!DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.SpotifyAccess))
                return false;

            CurrentPlaybackContext currentPlayback =
                await new PlayerApi(new HttpClient(), GetAccessToken()).GetCurrentPlaybackInfo();

            return DataValidator.ValidateData(currentPlayback) && currentPlayback.IsPlaying;
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
                Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime =
                    DateTimeOffset.Now.DateTime.AddHours(1);
                Core.INSTANCE.SettingManager.WriteSettings();
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }

        public async Task StartAuthorization()
        {
            CefAuthWindow cefAuthWindow = new CefAuthWindow(this._baseAuthUrl, "/callback", "code");
            
            cefAuthWindow.Width = 500;
            cefAuthWindow.Height = 600;
            
            await cefAuthWindow.ShowDialog<string>(MainWindow.Instance);
            string token = await cefAuthWindow.GetAuthCode();
            
            /*
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo(url);
                processStartInfo.UseShellExecute = true;
                Process.Start(processStartInfo);
            }

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
            */

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

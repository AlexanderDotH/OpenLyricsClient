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
using DevBaseApi.Apis.OpenLyricsClient.Structure.Json;
using DevBaseFormat;
using DevBaseFormat.Formats.EnvFormat;
using DevBaseFormat.Structure;
using Microsoft.Extensions.Configuration;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Environment;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.External.CefNet.Structure;
using OpenLyricsClient.External.CefNet.Utils;
using OpenLyricsClient.External.CefNet.View;
using OpenLyricsClient.Frontend.View.Windows;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Backend.Handler.Services.Services.Spotify
{
    class SpotifyService : IService
    {
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
                    if (Core.INSTANCE.SettingManager.Settings.SpotifyAccess.AccessToken != null)
                    {
                        long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        long settings = Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime;
                        
                        if (now > settings)
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
            return Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected && Core.INSTANCE.SettingManager.Settings.SpotifyAccess.AccessToken != null;
        }

        public async Task<bool> TestConnection()
        {
            if (!IsConnected())
                return false;

            if (!DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.SpotifyAccess))
                return false;

            try
            {
                CurrentlyPlayingContext currentlyPlayingContext = await new SpotifyClient(GetAccessToken()).Player.GetCurrentPlayback();
                return DataValidator.ValidateData(currentlyPlayingContext) && currentlyPlayingContext.IsPlaying;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string GetAccessToken()
        {
            // Task.Factory.StartNew(async() =>
            // {
            //     await RefreshTokenRequest();
            //
            // }).Wait(Core.INSTANCE.CancellationTokenSource.Token);
            return Core.INSTANCE.SettingManager.Settings.SpotifyAccess.AccessToken;
        }

        private async Task RefreshTokenRequest()
        {
            try
            {
                DevBaseApi.Apis.OpenLyricsClient.OpenLyricsClient api =
                    new DevBaseApi.Apis.OpenLyricsClient.OpenLyricsClient();

                JsonOpenLyricsClientAccess access =
                        await api.GetAccessToken(Core.INSTANCE.SettingManager.Settings.SpotifyAccess.RefreshToken);
                
                Core.INSTANCE.SettingManager.Settings.SpotifyAccess.AccessToken = access.AccessToken;
                Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime = 
                    DateTimeOffset.Now.AddHours(1).ToUnixTimeMilliseconds();
                Core.INSTANCE.SettingManager.WriteSettings();
            }
            catch (Exception e)
            {
                this._debugger.Write(e);
            }
        }

        public async Task StartAuthorization()
        {
            Token token = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                CefAuthWindow cefAuthWindow = new CefAuthWindow("https://openlyricsclient.com/connect/spotify/begin", "/complete");
             
                cefAuthWindow.Width = 500;
                cefAuthWindow.Height = 600;
                cefAuthWindow.Title = "Connect to spotify";
             
                cefAuthWindow.ShowDialog<string>(MainWindow.Instance);
             
                token = await cefAuthWindow.GetAuthCode();
             
                cefAuthWindow.Close();
            } 
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("https://www.openlyricsclient.com/connect/spotify/begin/listener");
                processStartInfo.UseShellExecute = true;
                Process.Start(processStartInfo);
                
                Listener.Listener l = new Listener.Listener("http://127.0.0.1:45674/", "/complete", "refresh_token", "access_token");
                await l.StartListener();
                token = l.Response;
            }
            
            if (!DataValidator.ValidateData(token))
                return;
            
            SpotifyClient client = new SpotifyClient(token.AccessToken);
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.UserData = await client.UserProfile.Current();
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.AccessToken = token.AccessToken;
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.RefreshToken = token.RefreshToken;
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.SpotifyExpireTime =
                DateTimeOffset.Now.AddHours(1).ToUnixTimeMilliseconds();
            Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected = true;
            Core.INSTANCE.SettingManager.WriteSettings();
            
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
             

             /*BearerAccessRefreshToken bearerAccessRefresh = await _userAccountsService.RequestAccessRefreshToken(token);

             Core.INSTANCE.SettingManager.Settings.SpotifyAccess.RefreshToken = bearerAccessRefresh.RefreshToken;
             Core.INSTANCE.SettingManager.Settings.SpotifyAccess.BearerAccess = bearerAccessRefresh;
             Core.INSTANCE.SettingManager.Settings.SpotifyAccess.IsSpotifyConnected = true;
             Core.INSTANCE.SettingManager.WriteSettings();*/
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

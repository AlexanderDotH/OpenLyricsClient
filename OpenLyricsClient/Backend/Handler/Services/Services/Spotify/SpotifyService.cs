using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Threading;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Json;
using DevBase.Async.Task;
using DevBase.Generics;
using Microsoft.Extensions.Configuration;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Settings.Sections.Connection.Spotify;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Other;
using OpenLyricsClient.Backend.Utils;
using OpenLyricsClient.Frontend.Structure;
using OpenLyricsClient.Frontend.View.Windows;
using OpenLyricsClient.Frontend.View.Windows.Auth;
using SpotifyAPI.Web;
using SimpleArtist = OpenLyricsClient.Backend.Structure.Other.SimpleArtist;
using SimpleTrack = OpenLyricsClient.Backend.Structure.Other.SimpleTrack;

namespace OpenLyricsClient.Backend.Handler.Services.Services.Spotify
{
    class SpotifyService : IService
    {
        private IConfiguration _configurationManager;

        private TaskSuspensionToken _refreshTokenSuspensionToken;

        private Debugger<SpotifyService> _debugger;
        private bool _disposed;

        public SpotifyService()
        {
            this._debugger = new Debugger<SpotifyService>(this);
            this._disposed = false;

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

                if (!IsConnected())
                    continue;
                
                long now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long settings = Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                    .GetValue<long>("SpotifyExpireTime");
                        
                if (now > settings)
                {
                    await RefreshTokenRequest();
                    this._debugger.Write("Refreshed Spotify Token", DebugType.DEBUG);
                }
            }
        }

        public bool IsConnected()
        {
            return Core.INSTANCE.SettingsHandler.Settings<SpotifySection>().GetValue<bool>("IsSpotifyConnected") == true;
        }

        public async Task<bool> TestConnection()
        {
            if (!IsConnected())
                return false;

            if (!DataValidator.ValidateData(GetAccessToken()))
                return false;

            try
            {
                CurrentlyPlayingContext currentlyPlayingContext = await new SpotifyClient(GetAccessToken()).Player.GetCurrentPlayback();
                return DataValidator.ValidateData(currentlyPlayingContext);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<SpotifyStatistics> GetStatistics(string accessToken = "")
        {
            SpotifyClient client = new SpotifyClient(accessToken.Equals(String.Empty) ? GetAccessToken() : accessToken);

            PersonalizationTopRequest topRequest = new PersonalizationTopRequest
            {
                Limit = 5
            };
            
            Paging<FullArtist> topArtistsResponse = await client.Personalization.GetTopArtists(topRequest);
            Paging<FullTrack> topTracksResponse = await client.Personalization.GetTopTracks(topRequest);

            AList<FullArtist> topArtists = new AList<FullArtist>(topArtistsResponse.Items!.ToList());
            AList<FullTrack> topTracks = new AList<FullTrack>(topTracksResponse.Items!.ToList());

            SpotifyStatistics statistics = new SpotifyStatistics
            {
                TopArtists = SimpleArtist.ConvertTo(topArtists),
                TopTracks = SimpleTrack.ConvertTo(topTracks)
            };

            return statistics;
        }

        public async Task UpdatePlayback(EnumPlayback playback)
        {
            SpotifyClient spotifyClient = new SpotifyClient(GetAccessToken());
            Structure.Song.Song song = Core.INSTANCE.SongHandler?.CurrentSong!;
            
            switch (playback)
            {
                case EnumPlayback.PREVOUS_TRACK:
                {
                    await spotifyClient.Player.SkipPrevious();
                    break;
                }
                case EnumPlayback.NEXT_TRACK:
                {
                    await spotifyClient.Player.SkipNext();
                    break;
                }
                case EnumPlayback.PAUSE:
                {
                    await spotifyClient.Player.PausePlayback();
                    break;
                }
                case EnumPlayback.RESUME:
                {
                    await spotifyClient.Player.ResumePlayback();
                    break;
                }
            }
        }
        
        public string GetAccessToken()
        {
            // Task.Factory.StartNew(async() =>
            // {
            //     await RefreshTokenRequest();
            //
            // }).Wait(Core.INSTANCE.CancellationTokenSource.Token);
            return Core.INSTANCE.SettingsHandler.Settings<SpotifySection>().GetValue<string>("AccessToken");
        }

        private async Task RefreshTokenRequest()
        {
            try
            {
                DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient api =
                    new DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient();

                JsonOpenLyricsClientAccess access =
                        await api.GetAccessToken(Core.INSTANCE.SettingsHandler.Settings<SpotifySection>().GetValue<string>("RefreshToken"));
                
                SpotifyStatistics statistics = await GetStatistics(access.AccessToken);
               
                Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                    .SetValue("Statistics", statistics);
            
                Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                    .SetValue("AccessToken", access.AccessToken);
            
                Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                    .SetValue("SpotifyExpireTime", DateTimeOffset.Now.AddHours(1).ToUnixTimeMilliseconds());

                await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "Statistics");
                await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "AccessToken");
                await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "SpotifyExpireTime");
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
                CefAuthWindow cefAuthWindow = new CefAuthWindow("https://openlyricsclient.com/api/auth/spotify/begin", "/welcome");
             
                cefAuthWindow.Width = 1100;
                cefAuthWindow.Height = 850;
                cefAuthWindow.Title = "Connect to spotify";
             
                cefAuthWindow.ShowDialog(MainWindow.Instance);
             
                token = await cefAuthWindow.GetAuthCode();
             
                cefAuthWindow.Close();
            } 
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("https://openlyricsclient.com/api/auth/spotify/begin/listener");
                processStartInfo.UseShellExecute = true;
                Process.Start(processStartInfo);
                
                Listener.Listener l = new Listener.Listener("http://127.0.0.1:45674/", "/complete", "refresh_token", "access_token");
                await l.StartListener();
                token = l.Response;
            }
            
            if (!DataValidator.ValidateData(token))
                return;

            String t = token.RefreshToken;
            
            if (token.RefreshToken.EndsWith("&"))
                t = token.RefreshToken.Substring(0, token.RefreshToken.Length - 1);

            SpotifyClient client = new SpotifyClient(token.AccessToken);

            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("UserData", await client.UserProfile.Current());
            
            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("Statistics", await this.GetStatistics(token.AccessToken));
            
            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("AccessToken", token.AccessToken);
            
            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("RefreshToken", t);
            
            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("SpotifyExpireTime", DateTimeOffset.Now.AddHours(1).ToUnixTimeMilliseconds());

            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue<Boolean>("IsSpotifyConnected", true);

            await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "UserData");
            await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "Statistics");
            await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "AccessToken");
            await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "RefreshToken");
            await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "SpotifyExpireTime");
            await Core.INSTANCE.SettingsHandler.TriggerEvent(typeof(SpotifySection), "IsSpotifyConnected");
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

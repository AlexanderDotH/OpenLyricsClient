using System.Diagnostics;
using System.Runtime.InteropServices;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Json;
using DevBase.Async.Task;
using DevBase.Generics;
using Microsoft.Extensions.Configuration;
using OpenLyricsClient.Logic.Authentication;
using OpenLyricsClient.Logic.Debugger;
using OpenLyricsClient.Logic.Settings.Sections.Connection.Spotify;
using OpenLyricsClient.Shared.Structure.Access;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Other;
using OpenLyricsClient.Shared.Utils;
using SpotifyAPI.Web;
using SimpleArtist = OpenLyricsClient.Shared.Structure.Other.SimpleArtist;
using SimpleTrack = OpenLyricsClient.Shared.Structure.Other.SimpleTrack;

namespace OpenLyricsClient.Logic.Handler.Services.Services.Spotify
{
    public class SpotifyService : IService
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

                if (!Connected)
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

        public bool Connected => Core.INSTANCE.SettingsHandler.Settings<SpotifySection>().GetValue<bool>("IsSpotifyConnected");

        public async Task<bool> TestConnection()
        {
            if (!Connected)
                return false;

            if (!DataValidator.ValidateData(this.AccessToken))
                return false;

            try
            {
                CurrentlyPlayingContext currentlyPlayingContext = await new SpotifyClient(this.AccessToken).Player.GetCurrentPlayback();
                return DataValidator.ValidateData(currentlyPlayingContext);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool CanSeek()
        {
            return true;
        }

        public async Task<bool> Seek(long position)
        {
            if (!CanSeek())
                return false;
            
            if (!Connected)
                return false;

            if (!DataValidator.ValidateData(this.AccessToken))
                return false;

            try
            {
                return await new SpotifyClient(this.AccessToken).Player.SeekTo(new PlayerSeekToRequest(position));
            }
            catch (Exception e)
            {
                return false;
            }
        }
        
        public async Task<SpotifyStatistics> GetStatistics(string accessToken = "")
        {
            SpotifyClient client = new SpotifyClient(accessToken.Equals(String.Empty) ? this.AccessToken : accessToken);

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
            SpotifyClient spotifyClient = new SpotifyClient(this.AccessToken);
 
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

        public string AccessToken => Core.INSTANCE.SettingsHandler.Settings<SpotifySection>().GetValue<string>("AccessToken");

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
            AccessToken token = null;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string authFlow = 
                    await Core.INSTANCE.AuthenticationPipe.RequestAuthenticationWindow(
                        EnumAuthProvider.SPOTIFY,
                    "https://openlyricsclient.com/api/auth/spotify/begin", 
                        "/welcome", 
                        1100, 
                        850);

                AccessToken response = await Core.INSTANCE.AuthenticationPipe.GetToken<AccessToken>(authFlow);
                
                Core.INSTANCE.AuthenticationPipe.KillAuthWindow(authFlow);
                
                token = response;
            } 
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo("https://openlyricsclient.com/api/auth/spotify/begin/listener");
                processStartInfo.UseShellExecute = true;
                Process.Start(processStartInfo);
                
                Listener l = new Listener("http://127.0.0.1:45674/", "/complete", "refresh_token", "access_token");
                await l.StartListener();
                token = l.Response;
            }
            
            if (!DataValidator.ValidateData(token))
                return;

            String t = token.Refresh;
            
            if (token.Refresh.EndsWith("&"))
                t = token.Refresh.Substring(0, token.Refresh.Length - 1);

            SpotifyClient client = new SpotifyClient(token.Access);

            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("UserData", await client.UserProfile.Current());
            
            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("Statistics", await this.GetStatistics(token.Access));
            
            Core.INSTANCE.SettingsHandler.Settings<SpotifySection>()
                .SetValue("AccessToken", token.Access);
            
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
        
        public bool Active { get; set; }

        public string Name => "Spotify";

        string IService.ProcessName => "Spotify";

        public void Dispose()
        {
            this._disposed = true;

            Core.INSTANCE.TaskRegister.Kill(EnumRegisterTypes.SPOTIFY_REFRESHTOKEN);
        }
    }
}

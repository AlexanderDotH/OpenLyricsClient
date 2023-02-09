using System;
using System.Net;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Web;
using DevBase.Web.ResponseData;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Json.NetEase.Json;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Backend.Collector.Song.Providers.NetEase
{
    public class NetEaseCollector : ISongCollector
    {
        private readonly string _baseUrl;

        private Debugger<NetEaseCollector> _debugger;

        private const int RETRIES = 5;
        private const double RETRY_DURATION_MULTIPLIER = 20f;

        public NetEaseCollector()
        {
            this._baseUrl = "https://music.163.com/api";

            _debugger = new Debugger<NetEaseCollector>(this);
        }

        public async Task<SongResponseObject> GetSong(SongRequestObject songRequestObject)
        {
            if (!(DataValidator.ValidateData(songRequestObject) &&
                  DataValidator.ValidateData(songRequestObject.Artists, songRequestObject.SongDuration,
                      songRequestObject.SongName, songRequestObject.Album)))
                return null;
            
            NetEaseSearchResponse response = await SearchTrack(songRequestObject);

            if (!(DataValidator.ValidateData(response.Code, response.NetEaseResultDataResponse)))
                return null;

            if (response.Code != 200)
                return null;

            if (response.NetEaseResultDataResponse.Songs.IsNullOrEmpty())
                return null;

            if (response.NetEaseResultDataResponse.Songs.Length <= 0)
                return null;
            
            this._debugger.Write("Found " + response.NetEaseResultDataResponse.Songs.Length + " songs!", DebugType.INFO);
            
            int retryPercentage = 5;

            GenericList<NetEaseSongResponse> songs = new GenericList<NetEaseSongResponse>();

            for (int i = 0; i < RETRIES; i++)
            {
                for (int j = 0; j < response.NetEaseResultDataResponse.Songs.Length; j++)
                {
                    NetEaseSongResponse songResponse = response.NetEaseResultDataResponse.Songs[j];

                    if (!(DataValidator.ValidateData(songResponse)))
                        continue;
                    
                    if (songResponse.Artists.IsNullOrEmpty())
                        continue;
                    
                    if (!(DataValidator.ValidateData(songResponse.Name, songResponse.Id, songResponse.NetEaseAlbumResponse, songResponse.Duration)))
                        continue;

                    if (IsSongValid(songResponse, songRequestObject, retryPercentage))
                    {
                        songs.Add(songResponse);
                    }
                }

                retryPercentage = (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);
            }

            if (songs.Length > 0)
            {
                SongResponseObject songResponseObject = new SongResponseObject
                {
                    SongRequestObject = songRequestObject,
                    Track = songs,
                    CollectorName = this.CollectorName()
                };
                
                this._debugger.Write("Got current song " + songRequestObject.SongName + "!", DebugType.INFO);

                return songResponseObject;
            }

            return null;
        }
        
        private bool IsSongValid(NetEaseSongResponse songResponse, SongRequestObject songRequestObject, int percentage)
        {
            if (!DataValidator.ValidateData(songResponse))
                return false;

            if (!(songResponse.Name.Equals(songRequestObject.SongName) ||
                  songResponse.Name.Equals(songRequestObject.FormattedSongName)))
                return false;

            if (!(songResponse.NetEaseAlbumResponse.Name.Equals(songRequestObject.Album) ||
                  songResponse.NetEaseAlbumResponse.Name.Equals(songRequestObject.FormattedSongAlbum)))
                return false;

            if (!MatchDuration(songResponse, songRequestObject.SongDuration, percentage))
                return false;

            if (!MatchArtists(songResponse, DataConverter.ToArtists(songResponse.Artists), 100))
                return false;

            return true;
        }

        private bool MatchDuration(NetEaseSongResponse netEaseSongResponse, long duration, int percentage)
        {
            long songDurationThreshold = (long)((duration * 0.01) * percentage);
            return MathUtils.IsInRange(duration - songDurationThreshold, duration + songDurationThreshold, netEaseSongResponse.Duration);
        }

        private bool MatchArtists(NetEaseSongResponse netEaseSongResponse, string[] artists, double percentage)
        {
            double minArtistCount = Math.Floor((artists.Length * 0.01) * percentage);
            int artistsMatch = 0;

            if (artists.Length > 0)
            {
                for (int i = 0; i < netEaseSongResponse.Artists.Length; i++)
                {
                    for (int j = 0; j < artists.Length; j++)
                    {
                        if (netEaseSongResponse.Artists[i].Name == artists[j])
                        {
                            artistsMatch++;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < netEaseSongResponse.Artists.Length; i++)
                {
                    if (netEaseSongResponse.Artists[i].Name == artists[0])
                    {
                        artistsMatch++;
                    }
                }
            }

            return artistsMatch >= minArtistCount;
        }

        private async Task<NetEaseSearchResponse> SearchTrack(SongRequestObject songRequestObject)
        {
            string requestUrl = Uri.EscapeUriString(string.Format(
                "{0}/search/get?s={2}&type=1&offset=0&sub=false&limit=10",
                this._baseUrl, songRequestObject.GetArtistsSplit(), songRequestObject.SongName));

            //this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = await request.GetResponseAsync();

            //this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return new JsonDeserializer<NetEaseSearchResponse>().Deserialize(responseData.GetContentAsString());
            }

            return null;
        }

        public string CollectorName()
        {
            return "NetEase";
        }

        public int ProviderQuality()
        {
            return 3;
        }
    }
}

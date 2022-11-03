﻿using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Typography;
using DevBase.Utilities;
using DevBase.Web;
using DevBase.Web.ResponseData;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Structure;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase.Json;
using OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEaseV2.Json;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.NetEase
{
    class NetEaseCollector : ICollector
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

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (DataValidator.ValidateData(songRequestObject) &&
                DataValidator.ValidateData(songRequestObject.Artists, songRequestObject.SongDuration,
                    songRequestObject.SongName, songRequestObject.Album))
            {
                NetEaseSearchResponse response = await SearchTrack(songRequestObject);

                if (DataValidator.ValidateData(response) &&
                    DataValidator.ValidateData(response.Code, response.NetEaseResultDataResponse))
                {
                    if (response.Code == 200)
                    {
                        if (DataValidator.ValidateData(response.NetEaseResultDataResponse) &&
                            DataValidator.ValidateData(response.NetEaseResultDataResponse.HasMore,
                                response.NetEaseResultDataResponse.SongCount, response.NetEaseResultDataResponse.Songs))
                        {
                            if (response.NetEaseResultDataResponse.Songs.Length > 0)
                            {
                                int retryPercentage = 5;

                                for (int i = 0; i < RETRIES; i++)
                                {
                                    for (int j = 0; j < response.NetEaseResultDataResponse.Songs.Length; j++)
                                    {
                                        NetEaseSongResponse songResponse = response.NetEaseResultDataResponse.Songs[j];

                                        if (DataValidator.ValidateData(songResponse) && DataValidator.ValidateData(
                                                songResponse.Name, songResponse.Artists, songResponse.Alias,
                                                songResponse.Duration, songResponse.Id, songResponse.NetEaseAlbumResponse,
                                                songResponse.Alias, songResponse.Status))
                                        {
                                            SongMetadata songMetadata = SongMetadata.ToSongMetadata(
                                                songResponse.Name,
                                                songResponse.NetEaseAlbumResponse.Name,
                                                DataConverter.ToArtists(songResponse.Artists),
                                                songResponse.Duration);

                                            if (!IsSongValid(songResponse, songRequestObject, retryPercentage))
                                                continue;

                                            GenericList<Tuple<NetEaseSongResponse, NetEaseLyricResponse>> lyrics =
                                                new GenericList<Tuple<NetEaseSongResponse, NetEaseLyricResponse>>();
                                            
                                            int songId = songResponse.Id;
                                            NetEaseLyricResponse lyricResponse = await GetLyricsFromEndpoint(songId);

                                            if (DataValidator.ValidateData(lyricResponse) &&
                                                DataValidator.ValidateData(lyricResponse.Code,
                                                    lyricResponse.NetEaseLrcResponse))
                                            {

                                                if (songRequestObject.SelectioMode == SelectionMode.QUALITY)
                                                {
                                                    lyrics.Add(new Tuple<NetEaseSongResponse, NetEaseLyricResponse>(songResponse, lyricResponse));

                                                    for (int k = 0; k < lyrics.Length; k++)
                                                    {
                                                        Tuple<NetEaseSongResponse, NetEaseLyricResponse> lyricElement = lyrics.Get(k);
                                                        if (!IsGarbage(lyricElement.Item2))
                                                        {
                                                            return await ParseLyricResponse(lyricElement.Item2, songMetadata);
                                                        }
                                                    }
                                                }
                                                else if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
                                                {
                                                    return await ParseLyricResponse(lyricResponse, songMetadata);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            return new LyricData();
                                        }
                                    }

                                    retryPercentage = (int)Math.Ceiling(i * RETRY_DURATION_MULTIPLIER);
                                }
                            }
                            else
                            {
                                return new LyricData();
                            }
                        }
                    }
                }

                return new LyricData();
            }

            return new LyricData();
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

        private bool IsGarbage(NetEaseLyricResponse lyrics)
        {
            AString values = new AString(lyrics.NetEaseLrcResponse.Lyric);

            GenericList<string> lines = values.AsList();

            for (int i = 0; i < lines.Length; i++)
            {
                string element = lines.Get(i);

                if (!Regex.IsMatch(element, 
                        DevBaseFormat.Structure.RegexHolder.REGEX_GARBAGE))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<LyricData> ParseLyricResponse(NetEaseLyricResponse lyricResponse, SongMetadata songMetadata)
        {
            if (DataValidator.ValidateData(lyricResponse) &&
                DataValidator.ValidateData(lyricResponse.Code, lyricResponse.NetEaseLrcResponse.Lyric))
            {
                if (lyricResponse.Code == 200)
                {
                    if (lyricResponse.NetEaseLrcResponse.Lyric != null &&
                        lyricResponse.NetEaseLrcResponse.Version != 0)
                    {
                        FileFormatParser<LrcObject> fileFormatParser =
                            new FileFormatParser<LrcObject>(
                                new LrcParser<LrcObject>());

                        if (DataValidator.ValidateData(fileFormatParser))
                        {
                            GenericList<LyricElement> lyricElements =
                                fileFormatParser.FormatFromString(lyricResponse.NetEaseLrcResponse.Lyric).Lyrics;

                            if (DataValidator.ValidateData(lyricElements))
                            {
                                return await LyricData.ConvertToData(lyricElements, songMetadata, this.CollectorName());
                            }
                        }
                    }
                }
            }

            return new LyricData();
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

            this._debugger.Write("Full track search URL: " + requestUrl, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestUrl);
            ResponseData responseData = await request.GetResponseAsync();

            this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return new JsonDeserializer<NetEaseSearchResponse>().Deserialize(responseData.GetContentAsString());
            }

            return null;
        }

        private async Task<NetEaseLyricResponse> GetLyricsFromEndpoint(int songId)
        {
            string requestURL = Uri.EscapeUriString(string.Format("{0}/song/lyric?tv=-1&kv=-1&lv=-1&os=pc&id={1}", this._baseUrl, songId));

            this._debugger.Write("Full lyric fetch URL: " + requestURL, DebugType.DEBUG);

            DevBase.Web.Request request = new Request(requestURL);
            ResponseData responseData = await request.GetResponseAsync();

            this._debugger.Write(responseData.GetContentAsString(), DebugType.DEBUG);
            if (responseData.StatusCode == HttpStatusCode.OK)
            {
                return new JsonDeserializer<NetEaseLyricResponse>().Deserialize(responseData.GetContentAsString());
            }

            return null;
        }

        public string CollectorName()
        {
            return "NetEase";
        }

        public int ProviderQuality()
        {
            return (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.PERFORMANCE ? 4 : 3);
        }
    }
}

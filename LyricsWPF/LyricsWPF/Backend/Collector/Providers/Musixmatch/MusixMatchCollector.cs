using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBase.Web;
using DevBase.Web.RequestData;
using DevBase.Web.ResponseData;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Formats.MmlFormat;
using DevBaseFormat.Structure;
using LyricsWPF.Backend.Collector.Providers.Musixmatch.Json;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch
{
    class MusixMatchCollector : ICollector
    {

        private string _baseUrl;

        private Debugger<MusixMatchCollector> _debugger;

        public MusixMatchCollector()
        {
            this._debugger = new Debugger<MusixMatchCollector>(this);

            this._baseUrl = "https://apic-desktop.musixmatch.com";
        }

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return null;

            MusixMatchFetchResponse fetchedLyrics = await FetchTrack(songRequestObject);

            if (!DataValidator.ValidateData(fetchedLyrics))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body, fetchedLyrics.Message.Header))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.MatcherTrackGet))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.MatcherTrackGet.Message))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.MatcherTrackGet.Message.Body, fetchedLyrics.Message.Body.MacroCalls.MatcherTrackGet.Message.Header))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.MatcherTrackGet.Message.Body.Track))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.TrackSubtitlesGet))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.TrackSubtitlesGet.Message))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.TrackSubtitlesGet.Message.Header, fetchedLyrics.Message.Body.MacroCalls.TrackSubtitlesGet.Message.Body))
                return null;

            if (!DataValidator.ValidateData(fetchedLyrics.Message.Body.MacroCalls.TrackSubtitlesGet.Message.Body.SubtitleList))
                return null;

            if (fetchedLyrics.Message.Body.MacroCalls.TrackSubtitlesGet.Message.Header.Instrumental == 1)
                return new LyricData(LyricReturnCode.Success, new LyricPart[] { new LyricPart(0, "Instrumental") });

            MusixMatchSubtitleList[] subtitleList =
                fetchedLyrics.Message.Body.MacroCalls.TrackSubtitlesGet.Message.Body.SubtitleList;

            for (int i = 0; i < subtitleList.Length; i++)
            {
                MusixMatchSubtitle subtitle = subtitleList[i].Subtitle;

                if (!DataValidator.ValidateData(subtitle))
                    continue;

                if (subtitle.SubtitleBody == "")
                    continue;

                FileFormatParser<LrcObject> fileFormatParser =
                    new FileFormatParser<LrcObject>(
                        new MmlParser<LrcObject>());

                if (!DataValidator.ValidateData(fileFormatParser))
                    return null;

                GenericList<LyricElement> lyricElements =
                    fileFormatParser.FormatFromString(subtitle.SubtitleBody).Lyrics;

                if (!DataValidator.ValidateData(lyricElements))
                    return null;

                return LyricData.ConvertToData(lyricElements);
            }

            return null;
        }

        public async Task<MusixMatchFetchResponse> FetchTrack(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return null;

            if (!DataValidator.ValidateData(songRequestObject.SongName, songRequestObject.Artists,
                    songRequestObject.Album))
                return null;

            string requestString = Uri.EscapeUriString(
                string.Format("{0}/ws/1.1/macro.subtitles.get" +
                              "?format=json" +
                              "&user_language=en" +
                              "&namespace=lyrics_synched" +
                              "&f_subtitle_length_max_deviation=1" +
                              "&subtitle_format=mxm" +
                              "&app_id=web-desktop-app-v1.0" +
                              "&usertoken=190511307254ae92ff84462c794732b84754b64a2f051121eff330" +
                              "&q_track={1}" +
                              "&q_artist={2}" +
                              "&q_album={3}", 
                    this._baseUrl, songRequestObject.SongName, songRequestObject.GetArtistsSplit(), songRequestObject.Album));

            this._debugger.Write("Full track fetch url: " + requestString, DebugType.DEBUG);

            RequestData requestData = new RequestData(requestString);
            requestData.Header.Add("Cookie", "AWSELB=55578B011601B1EF8BC274C33F9043CA947F99DCFF0A80541772015CA2B39C35C0F9E1C932D31725A7310BCAEB0C37431E024E2B45320B7F2C84490C2C97351FDE34690157");
            requestData.UserAgent = requestData.GetRandomUseragent();

            Request request = new Request(requestData);
            ResponseData responseData = await request.GetResponseAsync();

            if (!DataValidator.ValidateData(responseData))
                return null;

            this._debugger.Write("Full track response data: " + responseData.GetContentAsString(), DebugType.DEBUG);

            if (responseData.GetContentAsString().Contains("\"hint\":\"captcha\""))
                return null;

            return JsonConvert.DeserializeObject<MusixMatchFetchResponse>(responseData.GetContentAsString());
        }

        public string CollectorName()
        {
            return "Musixmatch";
        }

        public int ProviderQuality()
        {
            return 10;
        }
    }
}

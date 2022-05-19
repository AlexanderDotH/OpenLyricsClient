using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Web;
using DevBase.Web.RequestData;
using DevBase.Web.ResponseData;
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

            try
            {
                MusixMatchRoot fetchedLyrics = await FetchTrack(songRequestObject);
            }
            catch (Exception e)
            {
                throw;
            }

            //if (!DataValidator.ValidateData(fetchedLyrics))
            //    return null;

            //if (!DataValidator.ValidateData(fetchedLyrics.RootMessage))
            //    return null;

            //if (!DataValidator.ValidateData(fetchedLyrics.RootMessage.RootBody, fetchedLyrics.RootMessage.RootHeader))
            //    return null;

            //Console.WriteLine(fetchedLyrics.RootMessage.RootBody.MacroCalls.TrackSubtitlesGet.Message.Body.Subtitle.Subtitle.SubtitleBody);

            return null;
        }

        public async Task<MusixMatchRoot> FetchTrack(SongRequestObject songRequestObject)
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
            
            Request request = new Request(requestData);
            ResponseData responseData = await request.GetResponseAsync();

            if (!DataValidator.ValidateData(responseData))
                return null;

            this._debugger.Write("Full track response data: " + responseData.GetContentAsString(), DebugType.DEBUG);

            return JsonConvert.DeserializeObject<MusixMatchRoot>(responseData.GetContentAsString());
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

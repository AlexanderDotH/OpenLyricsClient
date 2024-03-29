﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using DevBase.Api.Apis.Deezer.Structure.Json;
using DevBase.Api.Apis.Tidal.Structure.Json;
using OpenLyricsClient.Shared.Structure.Json.NetEase.Json;
using OpenLyricsClient.Shared.Structure.Json.NetEaseV2.Json;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Shared.Utils
{
    public class DataConverter
    {
        public static string GetArtistsSplit(string[] artists)
        {
            string returnArtists = string.Empty;

            for (int i = 0; i < artists.Length; i++)
            {
                returnArtists += i == 0 ? artists[i] : ", " + artists[i];
            }

            return returnArtists;
        }

        public static string[] TidalArtistsToString(List<JsonTidalArtist> artists)
        {
            string[] artistsAsStrings = new string[artists.Count];

            for (int i = 0; i < artists.Count; i++)
            {
                artistsAsStrings[i] = artists[i].Name;
            }

            return artistsAsStrings;
        }

        public static string[] SpotifyArtistsToStrings(List<SimpleArtist> artists)
        {
            string[] artistsAsStrings = new string[artists.Count];

            for (int i = 0; i < artists.Count; i++)
            {
                artistsAsStrings[i] = artists[i].Name;
            }

            return artistsAsStrings;
        }

        public static string[] ToArtists(NetEaseArtistResponse[] netEaseArtistResponses)
        {
            string[] artistsAsStrings = new string[netEaseArtistResponses.Length];

            for (int i = 0; i < netEaseArtistResponses.Length; i++)
            {
                artistsAsStrings[i] = netEaseArtistResponses[i].Name;
            }

            return artistsAsStrings;
        }
        
        public static string[] ToArtists(JsonDeezerSearchDataArtistResponse deezerArtists)
        {
            string[] artistsAsStrings = new string[1];
            artistsAsStrings[0] = deezerArtists.name;
            return artistsAsStrings;
        }

        public static string[] ToArtists(NetEaseV2ArtistResponse[] netEaseArtistResponses)
        {
            string[] artistsAsStrings = new string[netEaseArtistResponses.Length];

            for (int i = 0; i < netEaseArtistResponses.Length; i++)
            {
                artistsAsStrings[i] = netEaseArtistResponses[i].Name;
            }

            return artistsAsStrings;
        }
    }
}

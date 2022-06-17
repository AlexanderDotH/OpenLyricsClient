using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector.Providers.NetEase.Json;
using LyricsWPF.Backend.Collector.Providers.NetEaseV2.Json;
using SpotifyApi.NetCore;

namespace LyricsWPF.Backend.Utils
{
    class DataConverter
    {
        public static T[] ConvertTo<F,T>(F[] input)
        {
            T[] newArray = new T[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                newArray[i] = (T)(object)input[i];
            }

            return newArray;
        }

        public static string GetArtistsSplit(string[] artists)
        {
            string returnArtists = string.Empty;

            for (int i = 0; i < artists.Length; i++)
            {
                returnArtists += i == 0 ? artists[i] : ", " + artists[i];
            }

            return returnArtists;
        }

        public static string[] TidalArtistsToString(ObservableCollection<TidalLib.Artist> artists)
        {
            string[] artistsAsStrings = new string[artists.Count];

            for (int i = 0; i < artists.Count; i++)
            {
                artistsAsStrings[i] = artists[i].Name;
            }

            return artistsAsStrings;
        }

        public static string[] SpotifyArtistsToStrings(Artist[] artists)
        {
            string[] artistsAsStrings = new string[artists.Length];

            for (int i = 0; i < artists.Length; i++)
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

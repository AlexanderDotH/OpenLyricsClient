using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifyApi.NetCore;

namespace LyricsWPF.Backend.Utils
{
    class DataConverter
    {

        public static string[] SpotifyArtistsToStrings(Artist[] artists)
        {
            string[] artistsAsStrings = new string[artists.Length];

            for (int i = 0; i < artists.Length; i++)
            {
                artistsAsStrings[i] = artists[i].Name;
            }

            return artistsAsStrings;
        }
    }
}

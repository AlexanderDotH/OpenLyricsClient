using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Handler.Song
{
    class SongFormatter
    {

        public static string FormatSongName(string songName)
        {
            string[] regexes = new string[] { "(\\W- Radio Edit)", "\\(feat.*\\)", "\\(with .*\\)" };

            for (int i = 0; i < regexes.Length; i++)
            {
                Match r = Regex.Match(songName, regexes[i]);
                if (r.Success)
                {
                    return Regex.Split(songName, regexes[i])[0];
                }
            }

            return songName;
        }

    }
}

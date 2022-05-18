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
                    songName = songName.Replace(r.Value, string.Empty);
                }
            }

            return FormatString(songName.TrimEnd());
        }

        public static string FormatSongAlbum(string songAlbum)
        {
            string[] regexes = new string[] { "(\\W- Radio Edit)", "\\(feat.*\\)", "\\(with .*\\)" };

            for (int i = 0; i < regexes.Length; i++)
            {
                Match r = Regex.Match(songAlbum, regexes[i]);
                if (r.Success)
                {
                    songAlbum = songAlbum.Replace(r.Value, string.Empty);
                }
            }

            return FormatString(songAlbum.TrimEnd());
        }
        
        public static string FormatString(string s)
        {
            s = s.Replace("Λ", "/\\");
            s = s.Replace("（", "(");
            s = s.Replace("）", ")");
            s = s.Replace("【", "[");
            s = s.Replace("】", "]");
            s = s.Replace("。", ".");
            s = s.Replace("；", ";");
            s = s.Replace("：", ":");
            s = s.Replace("？", "?");
            s = s.Replace("！", "!");
            s = s.Replace("、", ",");
            s = s.Replace("，", ",");
            s = s.Replace("‘", "'");
            s = s.Replace("’", "'");
            s = s.Replace("′", "'");
            s = s.Replace("＇", "'");
            s = s.Replace("“", "\"");
            s = s.Replace("”", "\"");
            s = s.Replace("〜", "~");
            s = s.Replace("·", "•");
            s = s.Replace("・", "•");
            s = s.Replace("’", "´");
            return s;
        }
    }
}

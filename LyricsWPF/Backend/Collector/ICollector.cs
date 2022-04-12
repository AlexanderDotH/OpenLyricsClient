using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;

namespace LyricsWPF.Backend.Collector
{
    interface ICollector
    {
        LyricData GetLyrics(SongRequestObject songRequestObject);
        string CollectorName();
    }
}

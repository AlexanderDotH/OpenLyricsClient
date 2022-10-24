using System;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Structure.Lyrics
{
    [Serializable]
    public class LyricPart
    {
        private long _time;
        private string _part;

        public LyricPart(long time, string part)
        {
            this._time = time;
            this._part = part;
        }

        public long Time
        {
            get => this._time;
            set => this._time = value;
        }

        public string Part
        {
            get => this._part;
            set => this._part = value;
        }
    }
}

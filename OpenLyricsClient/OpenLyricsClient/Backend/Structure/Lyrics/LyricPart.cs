using System;

namespace OpenLyricsClient.Backend.Structure.Lyrics
{
    [Serializable]
    public class LyricPart
    {
        private long _time;
        private string _part;

        public LyricPart(long time, string part)
        {
            _time = time;
            _part = part;
        }

        public long Time
        {
            get => _time;
            set => _time = value;
        }

        public string Part
        {
            get => _part;
            set => _part = value;
        }

    }
}

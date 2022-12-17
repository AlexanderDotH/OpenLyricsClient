using System;
using Newtonsoft.Json;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Structure.Lyrics
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class LyricPart
    {
        private long _time;
        private string _part;

        public LyricPart(long time, string part)
        {
            this._time = time;
            this._part = part;
        }

        [JsonProperty]
        public long Time
        {
            get => this._time;
            set => this._time = value;
        }

        [JsonProperty]
        public string Part
        {
            get => this._part;
            set => this._part = value;
        }

        public bool Equals(LyricPart obj)
        {
            if (!DataValidator.ValidateData(obj))
                return false;
            
            return obj.Part.Equals(this.Part) && obj.Time.Equals(this.Time);
        }
    }
}

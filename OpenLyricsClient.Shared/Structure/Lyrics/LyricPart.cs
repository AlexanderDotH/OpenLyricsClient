using System;
using Newtonsoft.Json;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Shared.Structure.Lyrics
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class LyricPart
    {
        private long _time;
        private string _part;
        private double _percentage;

        public LyricPart(long time, string part)
        {
            this._time = time;
            this._part = part;
            this._percentage = 0;
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

        public double Percentage
        {
            get => _percentage;
            set => _percentage = value;
        }

        public bool Equals(LyricPart obj)
        {
            if (!DataValidator.ValidateData(obj))
                return false;

            if (!DataValidator.ValidateData(this))
                return false;
            
            return obj.Part.Equals(this.Part) && obj.Time.Equals(this.Time);
        }
    }
}

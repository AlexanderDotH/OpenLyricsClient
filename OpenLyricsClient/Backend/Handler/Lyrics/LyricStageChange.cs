using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Lyrics
{
    class LyricStageChange
    {

        private LyricData _lyricData;

        public LyricStageChange() {}

        public bool HasChanged(LyricData lyricData)
        {
            if (DataValidator.ValidateData(lyricData))
            {
                if (!DataValidator.ValidateData(this._lyricData))
                {
                    this._lyricData = lyricData;
                    return true;
                }
            }

            return false;
        }

    }
}

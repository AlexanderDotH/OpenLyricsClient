using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Lyrics
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

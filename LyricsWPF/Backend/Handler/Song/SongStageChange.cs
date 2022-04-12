using System;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Song
{
    class SongStageChange
    {
        private Song _lastSong;

        public SongStageChange()
        {
        }


        public bool HasSongChanged(Song currentSong)
        {
            if (DataValidator.ValidateData(currentSong))
            {
                if (this._lastSong == null)
                {
                    this._lastSong = currentSong;
                    return true;
                }

                if (currentSong.Title != this._lastSong.Title)
                {
                    this._lastSong = currentSong;
                    return true;
                }

                if (this._lastSong.MaxTime != currentSong.MaxTime)
                {
                    this._lastSong = currentSong;
                    return true;
                }

                double msSection = currentSong.MaxTime * 0.01;

                if (currentSong.Time < msSection)
                {
                    this._lastSong = currentSong;
                    return true;
                }
            }

            return false;
        }

        public bool HasSongStateChanged(Song currentSong)
        {
            double msSection = (int)currentSong.MaxTime * 0.1;

            if (!MathUtils.IsDoubleInRange(currentSong.Time - msSection, currentSong.Time + msSection, currentSong.Time))
            {
                return true;
            }

            return false;
        }
    }
}

using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Song
{
    class SongStageChange
    {
        private Structure.Song.Song _lastSong;
        private bool _timeCheck = false;

        public SongStageChange()
        {
        }


        public bool HasSongChanged(Structure.Song.Song currentSong)
        {
            if (DataValidator.ValidateData(currentSong))
            {
                if (this._lastSong == null)
                {
                    this._timeCheck = false;
                    this._lastSong = currentSong;
                    return true;
                }

                if (currentSong.Title != this._lastSong.Title)
                {
                    this._timeCheck = false;
                    this._lastSong = currentSong;
                    return true;
                }

                if (this._lastSong.MaxTime != currentSong.MaxTime)
                {
                    this._timeCheck = false;
                    this._lastSong = currentSong;
                    return true;
                }
                
                short msSection = (short)(currentSong.MaxTime * 0.01);

                if (currentSong.ProgressMs > msSection)
                {
                    this._timeCheck = true;
                }

                if (currentSong.ProgressMs < msSection && _timeCheck)
                {
                    this._timeCheck = false;
                    this._lastSong = currentSong;
                    return true;
                }

                //if (DataValidator.ValidateData(this._lastSong.Lyrics) &&
                //    DataValidator.ValidateData(currentSong.Lyrics))
                //{
                //    if (!currentSong.Lyrics.SongName.Equals(currentSong.Name))
                //    {
                //        this._lastSong = currentSong;
                //        return true;
                //    }
                //}
            }
            else
            {
                return true;
            }

            return false;
        }

        public void Reset()
        {
            this._lastSong = null;
            this._timeCheck = true;
        }
    }
}

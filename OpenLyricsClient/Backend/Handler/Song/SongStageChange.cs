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

                if (currentSong.SongMetadata.Name != this._lastSong.SongMetadata.Name)
                {
                    this._timeCheck = false;
                    this._lastSong = currentSong;
                    return true;
                }

                if (this._lastSong.SongMetadata.MaxTime != currentSong.SongMetadata.MaxTime)
                {
                    this._timeCheck = false;
                    this._lastSong = currentSong;
                    return true;
                }

                //if (DataValidator.ValidateData(this._lastSong.Lyrics, currentSong.Lyrics))
                //{
                //    if (this._lastSong.Lyrics != currentSong.Lyrics)
                //    {
                //        this._timeCheck = false;
                //        this._lastSong = currentSong;
                //        return true;
                //    }
                //}

                if (currentSong.Synced)
                {
                    short msSection = (short)(currentSong.SongMetadata.MaxTime * 0.01);

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

        public void Update(Structure.Song.Song song)
        {
            this._lastSong = song;
        }
    }
}

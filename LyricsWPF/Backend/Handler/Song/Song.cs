using System;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Song
{
    class Song
    {
        private const int LYRIC_OFFSET = 0;

        private string _title;
        private string[] _artists;
        private long _time;
        private long _maxTime;
        private bool _hasLyrics;
        private LyricData _lyrics;
        private LyricPart _currentLyricPart;

        private long _timeStamp;
        private bool _paused;

        private Debugger<Song> _debugger;

        public Song(string title, string[] artists)
        {
            this._debugger = new Debugger<Song>(this);

            this._title = title;
            this._artists = artists;
            this._time = 0;
            this._hasLyrics = false;

            //this._currentLyricPart = new LyricPart(0, "Error");
        }

        public void SyncTime()
        {
            if (DataValidator.ValidateData(this._timeStamp, this._paused))
            {
                if (!this._paused)
                    this._time = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this._timeStamp;
            }
        }

        public void UpdateLyricsToTime()
        {
            //await Task.Run(() =>
            //{
                if (DataValidator.ValidateData(this._lyrics) &&
                    DataValidator.ValidateData(this._time) &&
                    DataValidator.ValidateData(this._lyrics.LyricParts) &&
                    this._hasLyrics)
                {
                    for (int i = 0; i < this._lyrics.LyricParts.Length; i++)
                    {
                        LyricPart currentPart = this._lyrics.LyricParts[i];

                        if (i + 1 < this._lyrics.LyricParts.Length)
                        {
                            LyricPart nextPart = this._lyrics.LyricParts[i + 1];

                            if (MathUtils.IsInRange(currentPart.Time, nextPart.Time, this._time + LYRIC_OFFSET))
                            {
                                this._currentLyricPart = currentPart;
                                return;
                            }
                        }
                        else
                        {
                            this._currentLyricPart = this._lyrics.LyricParts[this._lyrics.LyricParts.Length - 1];
                            return;
                        }
                    }
                }
            //});
        }

        public LyricsRoll GetLyricsRoll()
        {
            if (DataValidator.ValidateData(this._lyrics, this._currentLyricPart) && this._hasLyrics)
            {
                for (int i = 0; i < this._lyrics.LyricParts.Length; i++)
                {
                    LyricPart secondLyricPart = this._lyrics.LyricParts[i];

                    if (secondLyricPart == this._currentLyricPart)
                    {
                        LyricPart firstLyricPart = null;
                        LyricPart thirdLyricPart = null;

                        if (i - 1 > 0)
                        {
                            firstLyricPart = this._lyrics.LyricParts[i - 1];
                        }

                        if (i + 1 < this._lyrics.LyricParts.Length)
                        {
                            thirdLyricPart = this._lyrics.LyricParts[i + 1];
                        }

                        LyricsRoll lyricsRoll = new LyricsRoll(firstLyricPart, secondLyricPart, thirdLyricPart);
                        return lyricsRoll;
                    }
                }
            }

            return null;
        }

        public void Dispose()
        {

        }

        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }

        public string[] Artists
        {
            get { return this._artists; }
            set { this._artists = value; }
        }

        public long Time
        {
            get { return this._time; }
            set
            {
                this._time = value;
            }
        }

        public long MaxTime
        {
            get { return this._maxTime; }
            set { this._maxTime = value; }
        }

        public bool HasLyrics
        {
            get { return this._hasLyrics; }
            set { this._hasLyrics = value; }
        }

        public LyricData Lyrics
        {
            get { return this._lyrics; }
            set
            {
                this._hasLyrics = true;
                this._lyrics = value; 
            }
        }

        public bool Paused
        {
            get => _paused;
            set => _paused = value;
        }

        public long TimeStamp
        {
            get => _timeStamp;
            set => _timeStamp = value;
        }

        public LyricPart CurrentLyricPart
        {
            get
            {
                if (!_hasLyrics)
                {
                    return new LyricPart(0, "Error");
                }
                
                return this._currentLyricPart;
            }
        }
    }
}

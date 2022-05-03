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
        private string _album;
        private long _time;
        private long _maxTime;
        private bool _hasLyrics;
        private LyricData _lyrics;
        private LyricPart _currentLyricPart;

        private long _timeStamp;
        private long _progressMS;
        private bool _paused;

        private Debugger<Song> _debugger;

        public Song(string title, string[] artists, long maxTime)
        {
            this._debugger = new Debugger<Song>(this);

            this._title = title;
            this._artists = artists;
            this._maxTime = maxTime;

            this._time = 0;
            this._hasLyrics = false;

            //this._currentLyricPart = new LyricPart(0, "Error");
        }

        public void SyncTime()
        {
            if (DataValidator.ValidateData(this._timeStamp, this._paused, this._progressMS))
            {
                if (!this._paused)
                {
                    long current_time = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                    long diff = 0;

                    if (this._timeStamp > 0)
                    {
                        diff = (current_time - _timeStamp);
                    }

                    this._time = _progressMS + diff;
                    this._timeStamp = 0;
                }
            }
        }

        //Garbage Collector cries, when calling this
        // Solutions:
        // Find issue and make it more performant
        // Add thread.sleep before calling this(less fun)
        public void UpdateLyricsToTime()
        {
            //await Task.Run(() =>
            //{
            if (DataValidator.ValidateData(this._lyrics) && 
                DataValidator.ValidateData(this._lyrics.LyricParts) &&
                DataValidator.ValidateData(this._time) &&
                this._hasLyrics)
            {
                for (int i = 0; i < this._lyrics.LyricParts.Length; i++)
                {
                    LyricPart currentPart = this._lyrics.LyricParts[i];

                    if (i + 1 < this._lyrics.LyricParts.Length)
                    {
                        LyricPart nextPart = this._lyrics.LyricParts[i + 1];

                        // I thing this is the issue
                        // What did I do?: nothing, cause I don´t now how to fix it
                        if (DataValidator.ValidateData(currentPart) && 
                            DataValidator.ValidateData(currentPart.Part, currentPart.Time) &&
                            DataValidator.ValidateData(nextPart) && 
                            DataValidator.ValidateData(nextPart.Part, nextPart.Time))
                        {
                            if (MathUtils.IsInRange(currentPart.Time, nextPart.Time, this._time + LYRIC_OFFSET))
                            {
                                this._currentLyricPart = currentPart;
                                return;
                            }
                        }
                    }
                    else
                    {
                        this._currentLyricPart = this._lyrics.LyricParts[this._lyrics.LyricParts.Length - 1];
                        return;
                    }
                }
            }
        }

        public LyricsRoll GetLyricsRoll()
        {
            if (DataValidator.ValidateData(this._lyrics) &&
                DataValidator.ValidateData(this._currentLyricPart) &&
                DataValidator.ValidateData(this._lyrics.LyricParts) &&
                this._hasLyrics)
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

        public string Title
        {
            get => _title;
            set => _title = value;
        }

        public string[] Artists
        {
            get => _artists;
            set => _artists = value;
        }

        public long Time
        {
            get => _time;
            set => _time = value;
        }

        public long MaxTime
        {
            get => _maxTime;
            set => _maxTime = value;
        }

        public bool HasLyrics
        {
            get => _hasLyrics;
            set => _hasLyrics = value;
        }

        public LyricData Lyrics
        {
            get => _lyrics;
            set => _lyrics = value;
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

        public long ProgressMs
        {
            get => _progressMS;
            set => _progressMS = value;
        }
        
        public string Album
        {
            get => _album;
            set => _album = value;
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
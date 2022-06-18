using System;
using System.Threading;
using System.Threading.Tasks;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Romanisation;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Utils;

namespace LyricsWPF.Backend.Handler.Song
{
    public class Song
    {
        private SongMetadata _songMetadata;
        private long _time;
        private LyricData _lyrics;
        private LyricPart _currentLyricPart;
        private LyricsRoll _currentLyricsRoll;
        private SongState _state;

        private bool _firstUpdate;

        private long _timeStamp;
        private long _progressMS;
        private long _startTime;
        private long _timeThreshold;
        private bool _paused;

        public Song(string title, string album, string[] artists, long maxTime)
        {
            this._songMetadata = new SongMetadata(title, album, artists, maxTime);

            this._startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this._firstUpdate = true;
            this._timeThreshold = 0;
            this._time = 0;
            this._state = SongState.NO_LYRICS_AVAILABLE;

            //this._currentLyricPart = new LyricPart(0, "Error");
        }

        public TimeSpan ProgressTimeSpan
        {
            get { return TimeSpan.FromMilliseconds(this._time); }
        }

        public TimeSpan MaxProgressTimeSpan
        {
            get { return TimeSpan.FromMilliseconds(this._songMetadata.MaxTime); }
        }

        public string ProgressString
        {
            get
            {
                return $"{(int)ProgressTimeSpan.TotalMinutes}:{ProgressTimeSpan.Seconds:00}";
            }
        }

        public string MaxProgressString
        {
            get
            {
                return $"{(int)MaxProgressTimeSpan.TotalMinutes}:{MaxProgressTimeSpan.Seconds:00}";
            }
        }

        public double GetPercentage()
        {
            double maxTime = this._songMetadata.MaxTime;
            double time = this._time;

            if (time == 0 || maxTime == 0)
                return 0;

            double divide = time / maxTime;

            if (Double.IsNaN(divide))
                return 0;

            return 100.0 * divide;
        }

        public SongMetadata SongMetadata
        {
            get => _songMetadata;
            set => _songMetadata = value;
        }

        public string Title
        {
            get => this._songMetadata.Name;
        }

        public string[] Artists
        {
            get => this._songMetadata.Artists;
        }

        public string FullArtists
        {
            get => this._songMetadata.FullArtists;
        }

        public string Album
        {
            get => this._songMetadata.Album;
        }

        public long Time
        {
            get => _time;
            set => _time = value;
        }

        public long MaxTime
        {
            get => this._songMetadata.MaxTime;
        }

        public LyricData Lyrics
        {
            get => _lyrics;

            set
            {
                _state = SongState.HAS_LYRICS_AVAILABLE;
                _lyrics = value;
            }
        }

        public SongState State
        {
            get => _state;
            set => _state = value;
        }

        public bool Paused
        {
            get => _paused;
            set => _paused = value;
        }

        public long TimeStamp
        {
            get => this._timeStamp;
            set
            {
                this._timeStamp = value;

                if (this._firstUpdate)
                {
                    this._timeThreshold = (long)(Math.Abs(this._startTime - DateTimeOffset.Now.ToUnixTimeMilliseconds()));
                    this._timeThreshold *= (long)(this._timeThreshold * 0.0095);

                    this._startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    this._firstUpdate = false;
                }
            }
        }

        public long ProgressMs
        {
            get => _progressMS;
            set
            {
                _progressMS = value;
            } 
        }

        public long StartTime
        {
            get => _startTime;
        }

        public LyricPart CurrentLyricPart
        {
            get
            {
                return this._currentLyricPart;
            }
            set
            {
                this._currentLyricPart = value;
            }
        }

        public long TimeThreshold
        {
            get => _timeThreshold;
        }

        public LyricsRoll CurrentLyricsRoll
        {
            get => _currentLyricsRoll;
            set => _currentLyricsRoll = value;
        }
    }
}
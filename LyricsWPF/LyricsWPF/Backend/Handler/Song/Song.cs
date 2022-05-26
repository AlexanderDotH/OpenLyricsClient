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
        private string _title;
        private string[] _artists;
        private string _album;
        private long _time;
        private long _maxTime;
        private LyricData _lyrics;
        private LyricPart _currentLyricPart;
        private LyricsRoll _currentLyricsRoll;
        private SongState _state;

        private long _timeStamp;
        private long _progressMS;
        private bool _paused;

        public Song(string title, string[] artists, long maxTime)
        {
            this._title = title;
            this._artists = artists;
            this._maxTime = maxTime;

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
            get { return TimeSpan.FromMilliseconds(this._maxTime); }
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
            return 100.0 * this._time / this._maxTime;
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
                return this._currentLyricPart;
            }
            set
            {
                this._currentLyricPart = value;
            }
        }

        public LyricsRoll CurrentLyricsRoll
        {
            get => _currentLyricsRoll;
            set => _currentLyricsRoll = value;
        }
    }
}
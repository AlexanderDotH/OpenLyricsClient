﻿using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Shared.Structure.Song
{
    [Serializable]
    public class Song
    {
        private SongMetadata _songMetadata;
        private long _time;
        private LyricData _lyrics;
        private LyricPart _currentLyricPart;
        private SongState _state;
        private Object _trackObject;
        private DataOrigin _dataOrigin;
        private EnumPlayback _playback;

        private bool _firstUpdate;

        private long _timeStamp;
        private long _progressMS;
        private long _startTime;
        private long _timeThreshold;
        private bool _paused;
        private bool _synced;

        private Artwork.Artwork _artwork;

        public Song(DataOrigin origin, Object trackObject, string title, string album, string[] artists, long maxTime)
        {
            this._songMetadata = new SongMetadata(title, album, artists, maxTime);

            this._dataOrigin = origin;
            this._trackObject = trackObject;
            this._startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            this._firstUpdate = true;
            this._timeThreshold = 0;
            this._time = 0;
            this._state = SongState.NO_LYRICS_AVAILABLE;
            this._synced = false;

            //this._currentLyricPart = new LyricPart(0, "Error");
        }

        public object TrackObject
        {
            get => _trackObject;
            set => _trackObject = value;
        }

        public DataOrigin DataOrigin
        {
            get => _dataOrigin;
            set => _dataOrigin = value;
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

        public float GetPercentage()
        {
            float maxTime = this._songMetadata.MaxTime;
            float time = this._time;

            if (time == 0 || maxTime == 0)
                return 0;

            double divide = time / maxTime;

            if (Double.IsNaN(divide))
                return 0;

            return (float)Math.Abs(100.0 * divide);
        }

        public SongMetadata SongMetadata
        {
            get => _songMetadata;
            set => _songMetadata = value;
        }

        public Artwork.Artwork Artwork
        {
            get => this._artwork;
            set => this._artwork = value;
        }

        public EnumPlayback Playback
        {
            get => _playback;
            set => _playback = value;
        }

        public long Time
        {
            get => _time;
            set
            {
                if (value < 0)
                    this._time = 0;

                if (value > this._songMetadata.MaxTime)
                    this._time = this._songMetadata.MaxTime;

                this._time = value;
                this._synced = true;
            }
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
            set
            {
                _paused = value;
            }
        }

        public long TimeStamp
        {
            get => this._timeStamp;
            set
            {
                this._timeStamp = value;
            }
        }

        public long ProgressMs
        {
            get => _progressMS;
            set
            {
                _progressMS = value;

                this._timeThreshold = (long)(Math.Abs(this._startTime - DateTimeOffset.Now.ToUnixTimeMilliseconds()));
                this._timeThreshold *= (long)(this._timeThreshold * 0.01);
                this._startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                if (this._timeThreshold > 550)
                    this._timeThreshold = 550;
            } 
        }

        public long StartTime
        {
            get => _startTime;
            set => _startTime = value;
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
            set => _timeThreshold = value;
        }

        public bool Synced
        {
            get => _synced;
        }
    }
}
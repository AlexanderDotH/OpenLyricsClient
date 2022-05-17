using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Collector;

namespace LyricsWPF.Backend.Handler.Song
{
    class SongRequestObject
    {
        private string _songName;
        private string[] _artists;
        private string _album;
        private long _songDuration;
        private SelectionMode _selectioMode;

        public SongRequestObject(string songName, string[] artists, long songDuration, string album, SelectionMode selectioMode)
        {
            _songName = songName;
            _artists = artists;
            _songDuration = songDuration;
            _album = album;
            _selectioMode = selectioMode;
        }

        public SongRequestObject(string songName) : this(songName, new string[] { }, 0, null, SelectionMode.PERFORMANCE) {}

        public SongRequestObject(string songName, string[] artists) : this(songName, artists, 0, null, SelectionMode.PERFORMANCE) { }

        public string SongName
        {
            get => _songName;
        }

        public string[] Artists
        {
            get => _artists;
        }

        public long SongDuration
        {
            get => _songDuration;
        }

        public string Album
        {
            get => _album;
            set => _album = value;
        }

        public SelectionMode SelectioMode
        {
            get => _selectioMode;
            set => _selectioMode = value;
        }

        public string GetArtistsSplit()
        {
            string artists = string.Empty;

            for (int i = 0; i < this._artists.Length; i++)
            {
                artists += i == 0 ? this._artists[i] : "," + this._artists[i];
            }

            return artists;
        }
    }
}

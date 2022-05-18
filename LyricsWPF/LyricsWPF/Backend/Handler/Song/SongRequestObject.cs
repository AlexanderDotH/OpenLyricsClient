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
        private string _formattedSongName;
        private string[] _artists;
        private string _album;
        private string _formattedSongAlbum;
        private long _songDuration;
        private SelectionMode _selectioMode;

        public SongRequestObject(string songName, string formattedSongName, string[] artists, long songDuration, string album, string formattedSongAlbum, SelectionMode selectioMode)
        {
            _songName = songName;
            _formattedSongName = formattedSongName;
            _artists = artists;
            _songDuration = songDuration;
            _album = album;
            _formattedSongAlbum = formattedSongAlbum;
            _selectioMode = selectioMode;
        }

        public string FormattedSongAlbum
        {
            get => _formattedSongAlbum;
        }

        public string FormattedSongName
        {
            get => _formattedSongName;
        }

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

using System;
using OpenLyricsClient.Backend.Collector.Lyrics;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Structure.Song
{
    [Serializable]
    public class SongRequestObject
    {
        private string _songName;
        private string _formattedSongName;
        private string[] _artists;
        private string _album;
        private string _formattedSongAlbum;
        private long _songDuration;
        private Song _song;

        public SongRequestObject(string songName, string formattedSongName, string[] artists, long songDuration, string album, string formattedSongAlbum)
        {
            _songName = songName;
            _formattedSongName = formattedSongName;
            _artists = artists;
            _songDuration = songDuration;
            _album = album;
            _formattedSongAlbum = formattedSongAlbum;
        }

        public SongRequestObject(Song song, string songName, string formattedSongName, string[] artists, long songDuration, string album, string formattedSongAlbum)
        {
            _song = song;
            _songName = songName;
            _formattedSongName = formattedSongName;
            _artists = artists;
            _songDuration = songDuration;
            _album = album;
            _formattedSongAlbum = formattedSongAlbum;
        }
        
        public SongRequestObject(string songName, string[] artists, long songDuration, string album) :
            this(songName, SongFormatter.FormatSongName(songName), artists, songDuration, album,
                SongFormatter.FormatSongAlbum(album)){}
       
        public static SongRequestObject FromSong(Song song)
        {
            if (!DataValidator.ValidateData(song))
                return null;
            
            SongRequestObject songRequestObject = new SongRequestObject(
                song,
                song.SongMetadata.Name,
                SongFormatter.FormatSongName(song.SongMetadata.Name),
                song.SongMetadata.Artists,
                song.SongMetadata.MaxTime,
                song.SongMetadata.Album,
                SongFormatter.FormatSongAlbum(song.SongMetadata.Album));

            return songRequestObject;
        }

        public Song Song
        {
            get => _song;
            set => _song = value;
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

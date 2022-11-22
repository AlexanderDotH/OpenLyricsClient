using System;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Structure.Song
{
    [Serializable]
    public class SongMetadata
    {
        private string _name;
        private string[] _artists;
        private string _fullArtists;
        private string _album;
        private long _maxTime;
        private string _artwork;

        public SongMetadata()
        {
            this._name = "Never gonna give you up";
            this._artists = new []{"Rick Astley"};
            this._fullArtists = DataConverter.GetArtistsSplit(new[] { "Rick Astley" });
            this._album = "Whenever You Need Somebody";
            this._maxTime = 199800;
        }

        public SongMetadata(string name, string album, string[] artists, long maxTime) : this()
        {
            this._name = name;
            this._album = album;
            this._artists = artists;
            this._fullArtists = DataConverter.GetArtistsSplit(artists);
            this._maxTime = maxTime;
        }

        public static SongMetadata ToSongMetadata(string title, string album, string[] artists, long maxTime)
        {
            return new SongMetadata(title, album, artists, maxTime);
        }

        public static SongMetadata ToSongMetadata(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return null;
            
            return new SongMetadata(songRequestObject.SongName,
                songRequestObject.Album, songRequestObject.Artists, songRequestObject.SongDuration);
        }

        public string Name
        {
            get => _name;
        }

        public string[] Artists
        {
            get => _artists;
        }

        public string FullArtists
        {
            get => _fullArtists;
        }

        public string Album
        {
            get => _album;
        }

        public string Artwork
        {
            get => _artwork;
            set => _artwork = value;
        }

        public long MaxTime
        {
            get => _maxTime;
        }
    }
}

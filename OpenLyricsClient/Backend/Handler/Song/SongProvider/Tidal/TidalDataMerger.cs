using System;
using DevBaseApi.Apis.Tidal.Structure.Json;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider.Tidal
{
    public class TidalDataMerger
    {
        public static Structure.Song.Song ValidateUpdatePlayBack(Structure.Song.Song song, JsonTidalTrack track)
        {
            if (!DataValidator.ValidateData(song))
                return song;

            if (!DataValidator.ValidateData(song.SongMetadata.Name, song.SongMetadata.Album, song.SongMetadata.Artists))
                return song;

            if (!DataValidator.ValidateData(track))
                return song;

            if (!DataValidator.ValidateData(track.Title, track.Album, track.Artists))
                return song;

            return UpdatePlayBack(song, track);
        }

        public static Structure.Song.Song UpdatePlayBack(Structure.Song.Song song, JsonTidalTrack track)
        {
            song.SongMetadata = SongMetadata.ToSongMetadata(
                track.Title, 
                track.Album.Title, 
                DataConverter.TidalArtistsToString(track.Artists),
                TimeSpan.FromSeconds(track.Duration).Milliseconds);
            
            return song;
        }

        public static Structure.Song.Song ValidateConvertAndMerge(JsonTidalTrack track)
        {
            if (!DataValidator.ValidateData(track))
                return null;

            if (!DataValidator.ValidateData(track.Title, track.Artists, track.Duration, track.Album))
                return null;

            return ConvertAndMerge(track);
        }

        public static Structure.Song.Song ConvertAndMerge(JsonTidalTrack track)
        {
            Structure.Song.Song song = new Structure.Song.Song(
                track.Title,
                track.Album.Title,
                DataConverter.TidalArtistsToString(track.Artists),
                TimeSpan.FromSeconds(track.Duration).Milliseconds);
            song.ProgressMs = 0;
            song.Time = -1;
            song.TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            song.Lyrics = null;
            song.CurrentLyricPart = null;
            song.State = SongState.SEARCHING_LYRICS;
            return song;
        }
    }
}

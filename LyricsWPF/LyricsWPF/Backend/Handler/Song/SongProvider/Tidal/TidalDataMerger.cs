using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Utils;
using TidalLib;

namespace LyricsWPF.Backend.Handler.Song.SongProvider.Tidal
{
    public class TidalDataMerger
    {
        public static Song ValidateUpdatePlayBack(Song song, Track track)
        {
            if (!DataValidator.ValidateData(song))
                return song;

            if (!DataValidator.ValidateData(song.Title, song.Album, song.Artists))
                return song;

            if (!DataValidator.ValidateData(track))
                return song;

            if (!DataValidator.ValidateData(track.Title, track.Album, track.Artists))
                return song;

            return UpdatePlayBack(song, track);
        }

        public static Song UpdatePlayBack(Song song, Track track)
        {
            song.Title = track.Title;
            song.Album = track.Album.Title;
            song.Artists = DataConverter.TidalArtistsToString(track.Artists);

            return song;
        }

        public static Song ValidateConvertAndMerge(Track track)
        {
            if (!DataValidator.ValidateData(track))
                return null;

            if (!DataValidator.ValidateData(track.Title, track.Artists, track.Duration, track.Album))
                return null;

            return ConvertAndMerge(track);
        }

        public static Song ConvertAndMerge(Track track)
        {
            Song song = new Song(
                track.Title,
                DataConverter.TidalArtistsToString(track.Artists),
                TimeSpan.FromSeconds(track.Duration).Milliseconds);
            song.Album = track.Album.Title;
            song.ProgressMs = 0;
            song.Time = 0;
            song.TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            song.MaxTime = (int)TimeSpan.FromSeconds(track.Duration).TotalMilliseconds;
            song.Lyrics = null;
            song.CurrentLyricPart = null;
            song.State = SongState.SEARCHING_LYRICS;
            return song;
        }
    }
}

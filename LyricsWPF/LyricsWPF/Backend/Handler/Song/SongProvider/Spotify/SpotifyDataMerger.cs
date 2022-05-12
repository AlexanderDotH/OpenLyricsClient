using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Utils;
using SpotifyApi.NetCore;

namespace LyricsWPF.Backend.Handler.Song.SongProvider.Spotify
{
    class SpotifyDataMerger
    {
        public static Song ValidateUpdatePlayBack(Song song, CurrentPlaybackContext playbackContext)
        {
            if (DataValidator.ValidateData(song) &&
                DataValidator.ValidateData(playbackContext) &&
                DataValidator.ValidateData(playbackContext.ProgressMs, playbackContext.Timestamp,
                    playbackContext.IsPlaying))
            {
                return UpdatePlayBack(song, playbackContext);
            }

            return song;
        }

        public static Song UpdatePlayBack(Song song, CurrentPlaybackContext playbackContext)
        {
            song.Paused = !playbackContext.IsPlaying;
            song.TimeStamp = playbackContext.Timestamp;

            if (playbackContext.ProgressMs.HasValue && playbackContext.IsPlaying)
            {
                song.ProgressMs = playbackContext.ProgressMs.Value;
            }

            return song;
        }

        public static Song ValidateUpdateAndMerge(Song song, CurrentTrackPlaybackContext currentTrack)
        {
            if (DataValidator.ValidateData(song) &&
                DataValidator.ValidateData(
                    song.Paused,
                    song.TimeStamp,
                    song.ProgressMs,
                    song.Artists,
                    song.Album))
            {
                if (DataValidator.ValidateData(currentTrack) &&
                    DataValidator.ValidateData(
                        currentTrack.IsPlaying,
                        currentTrack.Timestamp,
                        currentTrack.ProgressMs) &&
                    DataValidator.ValidateData(currentTrack.Item) &&
                    DataValidator.ValidateData(
                        currentTrack.Item.Artists,
                        currentTrack.Item.Album))
                {
                    return UpdateAndMerge(song, currentTrack);
                }
            }

            return song;
        }

        public static Song UpdateAndMerge(Song song, CurrentTrackPlaybackContext currentTrack)
        {
            song.Title = currentTrack.Item.Name;
            song.Artists = DataConverter.SpotifyArtistsToStrings(currentTrack.Item.Artists);
            return song;
        }

        public static Song ValidateConvertAndMerge(CurrentTrackPlaybackContext currentTrack)
        {
            if (DataValidator.ValidateData(currentTrack) &&
                DataValidator.ValidateData(currentTrack.Timestamp) &&
                DataValidator.ValidateData(currentTrack.Item) &&
                DataValidator.ValidateData(
                    currentTrack.Item.Name,
                    currentTrack.Item.Artists,
                    currentTrack.Item.DurationMs) &&
                DataValidator.ValidateData(currentTrack.ProgressMs) &&
                DataValidator.ValidateData(currentTrack.ProgressMs.Value))
            {
                return ConvertAndMerge(currentTrack);
            }

            return null;
        }

        public static Song ConvertAndMerge(CurrentTrackPlaybackContext currentTrack)
        {
            Song song = new Song(
                currentTrack.Item.Name,
                DataConverter.SpotifyArtistsToStrings(currentTrack.Item.Artists),
                currentTrack.Item.DurationMs);
            song.Album = currentTrack.Item.Album.Name;
            song.ProgressMs = currentTrack.ProgressMs.Value;
            song.TimeStamp = currentTrack.Timestamp;
            song.HasLyrics = false;
            song.Lyrics = null;
            song.CurrentLyricPart = null;
            return song;
        }
    }
}

using System;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;
using SpotifyApi.NetCore;

namespace OpenLyricsClient.Backend.Handler.Song.SongProvider.Spotify
{
    class SpotifyDataMerger
    {
        public static Structure.Song.Song ValidateUpdatePlayBack(Structure.Song.Song song, CurrentPlaybackContext playbackContext)
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

        public static Structure.Song.Song UpdatePlayBack(Structure.Song.Song song, CurrentPlaybackContext playbackContext)
        {
            song.Paused = !playbackContext.IsPlaying;

            if (!song.Paused)
            {
                song.TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                if (playbackContext.ProgressMs.HasValue) 
                    song.ProgressMs = playbackContext.ProgressMs.Value;
            }

            return song;
        }

        public static Structure.Song.Song ValidateUpdateAndMerge(Structure.Song.Song song, CurrentTrackPlaybackContext currentTrack)
        {
            if (DataValidator.ValidateData(song) &&
                DataValidator.ValidateData(
                    song.Paused,
                    song.TimeStamp,
                    song.ProgressMs,
                    song.SongMetadata.Artists,
                    song.SongMetadata.Album))
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

        public static Structure.Song.Song UpdateAndMerge(Structure.Song.Song song, CurrentTrackPlaybackContext currentTrack)
        {
            song.SongMetadata = SongMetadata.ToSongMetadata(
                currentTrack.Item.Name, 
                currentTrack.Item.Album.Name, 
                DataConverter.SpotifyArtistsToStrings(currentTrack.Item.Artists), 
                currentTrack.Item.DurationMs);
            return song;
        }

        public static Structure.Song.Song ValidateConvertAndMerge(CurrentTrackPlaybackContext currentTrack)
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

        public static Structure.Song.Song ConvertAndMerge(CurrentTrackPlaybackContext currentTrack)
        {
            Structure.Song.Song song = new Structure.Song.Song(
                currentTrack.Item.Name,
                currentTrack.Item.Album.Name,
                DataConverter.SpotifyArtistsToStrings(currentTrack.Item.Artists),
                currentTrack.Item.DurationMs);
            song.Time = -1;
            song.Lyrics = null;
            song.CurrentLyricPart = null;
            song.State = SongState.SEARCHING_LYRICS;
            return song;
        }
    }
}

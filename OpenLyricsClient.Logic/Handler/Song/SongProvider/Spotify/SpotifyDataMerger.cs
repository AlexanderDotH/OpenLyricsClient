using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;
using SpotifyAPI.Web;

namespace OpenLyricsClient.Logic.Handler.Song.SongProvider.Spotify
{
    class SpotifyDataMerger
    {
        public static Shared.Structure.Song.Song ValidateUpdatePlayBack(Shared.Structure.Song.Song song, CurrentlyPlayingContext playbackContext)
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

        public static Shared.Structure.Song.Song UpdatePlayBack(Shared.Structure.Song.Song song, CurrentlyPlayingContext playbackContext)
        {
            song.Paused = !playbackContext.IsPlaying;
            song.TimeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            song.ProgressMs = playbackContext.ProgressMs;
            return song;
        }

        public static Shared.Structure.Song.Song ValidateUpdateAndMerge(Shared.Structure.Song.Song song, CurrentlyPlayingContext currentTrack)
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
                    DataValidator.ValidateData(currentTrack.Item))
                {
                    return UpdateAndMerge(song, currentTrack);
                }
            }

            return song;
        }

        public static Shared.Structure.Song.Song UpdateAndMerge(Shared.Structure.Song.Song song, CurrentlyPlayingContext currentTrack)
        {
            if (currentTrack.Item.Type.Equals(ItemType.Track))
            {
                if (currentTrack.Item is FullTrack)
                {
                    FullTrack track = (FullTrack)currentTrack.Item;
                    song.SongMetadata = SongMetadata.ToSongMetadata(
                        track.Name, 
                        track.Album.Name, 
                        DataConverter.SpotifyArtistsToStrings(track.Artists), 
                        track.DurationMs);
                }
            }
           
            return song;
        }

        public static Shared.Structure.Song.Song ValidateConvertAndMerge(CurrentlyPlayingContext currentTrack)
        {
            if (DataValidator.ValidateData(currentTrack) &&
                DataValidator.ValidateData(currentTrack.Timestamp) &&
                DataValidator.ValidateData(currentTrack.Item) &&
                DataValidator.ValidateData(currentTrack.ProgressMs) &&
                DataValidator.ValidateData(currentTrack.ProgressMs))
            {
                return ConvertAndMerge(currentTrack);
            }

            return null;
        }

        public static Shared.Structure.Song.Song ConvertAndMerge(CurrentlyPlayingContext currentTrack)
        {
            if (currentTrack.Item.Type.Equals(ItemType.Track))
            {
                if (currentTrack.Item is FullTrack)
                {
                    FullTrack track = (FullTrack)currentTrack.Item;
                    Shared.Structure.Song.Song song = new Shared.Structure.Song.Song(
                        DataOrigin.SPOTIFY,
                        track,
                        track.Name,
                        track.Album.Name,
                        DataConverter.SpotifyArtistsToStrings(track.Artists),
                        track.DurationMs);
                    song.Time = -1;
                    song.Lyrics = null;
                    song.CurrentLyricPart = null;
                    song.State = SongState.SEARCHING_LYRICS;
                    return song;
                }
            }

            return null;
        }
    }
}

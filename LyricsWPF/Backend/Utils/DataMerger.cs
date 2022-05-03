using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Handler.Song;
using SpotifyApi.NetCore;

namespace LyricsWPF.Backend.Utils
{
    class DataMerger
    {
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
                        currentTrack.ProgressMs,
                        currentTrack.Item,
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
            song.Paused = !currentTrack.IsPlaying;
            song.TimeStamp = currentTrack.Timestamp;
            song.Artists = DataConverter.SpotifyArtistsToStrings(currentTrack.Item.Artists);
            
            if (currentTrack.ProgressMs.HasValue && currentTrack.IsPlaying)
            {
                song.ProgressMs = currentTrack.ProgressMs.Value;
            }

            return song;
        }

        public static Song ValidateConvertAndMerge(CurrentTrackPlaybackContext currentTrack)
        {
            if (DataValidator.ValidateData(currentTrack) &&
                DataValidator.ValidateData(
                    currentTrack.Item,
                    currentTrack.Item.Name,
                    currentTrack.Item.Artists,
                    currentTrack.Item.DurationMs,
                    currentTrack.ProgressMs,
                    currentTrack.ProgressMs.Value,
                    currentTrack.Timestamp))
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
            return UpdateAndMerge(song, currentTrack);
        }
    }
}

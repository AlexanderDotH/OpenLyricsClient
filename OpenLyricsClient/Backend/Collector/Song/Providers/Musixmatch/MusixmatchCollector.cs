﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
using DevBaseFormat.Formats.MmlFormat;
using DevBaseFormat.Structure;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Auth;
using MusixmatchClientLib.Types;
using OpenLyricsClient.Backend.Collector.Token.Provider.Musixmatch;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Song.Providers.Musixmatch
{
    public class MusixmatchCollector : ISongCollector
    {
        private Debugger<MusixmatchCollector> _debugger;

        public MusixmatchCollector()
        {
            this._debugger = new Debugger<MusixmatchCollector>(this);
        }
        
        public async Task<SongResponseObject> GetSong(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return null;

            string token = MusixmatchTokenCollector.Instance.GetToken().Token;

            if (!DataValidator.ValidateData(token))
                return null;

            MusixmatchClient musixmatchClient = new MusixmatchClient(token);

            if (!DataValidator.ValidateData(musixmatchClient))
                return null;

            List<Track> tracks = null;

            if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
            {
                tracks = await musixmatchClient.SongSearchAsync(
                    new TrackSearchParameters
                    {
                        Album = songRequestObject.Album,
                        Title = songRequestObject.SongName,
                        Artist = songRequestObject.GetArtistsSplit()
                    });
            }
            else
            {
                tracks = await musixmatchClient.SongSearchAsync(
                    new TrackSearchParameters
                    {
                        Album = songRequestObject.Album,
                        Title = songRequestObject.SongName,
                        Artist = songRequestObject.GetArtistsSplit()
                    });

                if (!DataValidator.ValidateData(tracks) || DataValidator.ValidateData(tracks) && tracks.Count == 0)
                {
                    tracks = await musixmatchClient.SongSearchAsync(
                        new TrackSearchParameters
                        {
                            Album = songRequestObject.FormattedSongAlbum,
                            Title = songRequestObject.SongName
                        });
                }
            }

            if (!DataValidator.ValidateData(tracks))
            {
                this._debugger.Write("Track not found", DebugType.ERROR);
                return null;
            }

            this._debugger.Write(string.Format("Found {0} tracks", tracks.Count), DebugType.INFO);

            for (int i = 0; i < tracks.Count; i++)
            {
                Track track = tracks[i];

                if (IsValidSong(track, songRequestObject))
                {
                    SongResponseObject songResponseObject = new SongResponseObject
                    {
                        SongRequestObject = songRequestObject,
                        Track = track,
                        CollectorName = this.CollectorName()
                    };

                    return songResponseObject;
                }
            }

            return null;
        }

        private bool IsValidSong(Track track, SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(track) ||
                !DataValidator.ValidateData(songRequestObject))
                return false;

            if (IsSimilar(songRequestObject.FormattedSongName, track.TrackName) != IsSimilar(songRequestObject.FormattedSongAlbum, track.AlbumName))
            {
                if (!IsSimilar(songRequestObject.FormattedSongAlbum, track.AlbumName))
                    return false;
            }

            //if ((track.TrackLength * 1000) != songRequestObject.SongDuration)
            //    return false;

            if (!IsSimilar(songRequestObject.FormattedSongName, track.TrackName))
                return false;

            if (!IsSimilar(songRequestObject.SongName, track.TrackName))
                return false;

            for (int i = 0; i < songRequestObject.Artists.Length; i++)
            {
                string artist = songRequestObject.Artists[i];

                if (track.ArtistName.Contains(artist))
                {
                    return true;
                }
            }

            return false;
        }

        //Untested! I should make everything a bit more strict
        private bool IsSimilar(string string1, string string2)
        {
            return MathUtils.CalculateLevenshteinDistance(string1, string2) >=
                   Math.Abs(string1.Length - string2.Length);
        }

        public string CollectorName()
        {
            return "MusixMatch";
        }

        public int ProviderQuality()
        {
            return (Core.INSTANCE.SettingManager.Settings.LyricSelectionMode == SelectionMode.PERFORMANCE ? 10 : 10); 
        }
    }
}
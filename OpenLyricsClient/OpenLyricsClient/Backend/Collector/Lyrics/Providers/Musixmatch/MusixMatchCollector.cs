using System;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.MmlFormat;
using DevBaseFormat.Structure;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Auth;
using MusixmatchClientLib.Types;
using OpenLyricsClient.Backend.Debugger;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Structure.Lyrics;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Lyrics.Providers.Musixmatch
{
    public class MusixMatchCollector : ICollector
    {
        private Debugger<MusixMatchCollector> _debugger;

        private TaskSuspensionToken _collectMusixMatchSuspensionToken;

        public MusixMatchCollector()
        {
            this._debugger = new Debugger<MusixMatchCollector>(this);
            
            Core.INSTANCE.TaskRegister.RegisterTask(
                out this._collectMusixMatchSuspensionToken, 
                new Task(async () => await this.CollectMusixMatchTokensTask(), Core.INSTANCE.CancellationTokenSource.Token, TaskCreationOptions.LongRunning), 
                EnumRegisterTypes.MUSIXMATCH_COLLECT_TOKENS);
        }

        private async Task CollectMusixMatchTokensTask()
        {
            while (!Core.IsDisposed())
            {
                await this._collectMusixMatchSuspensionToken.WaitForRelease();

                bool settingsChanged = false;

                try
                {
                    string token = await new MusixmatchToken("").IssueNewTokenAsync();
                    long expiresIn = DateTimeOffset.Now.AddMinutes(2).ToUnixTimeMilliseconds();

                    Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Add(MusixMatchToken.ToToken(token, expiresIn));

                    this._debugger.Write("Requested new musixmatch token", DebugType.INFO);

                    settingsChanged = true;
                }
                catch (Exception e) { }

                //Check expiration date
                for (int i = 0; i < Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Count; i++)
                {
                    MusixMatchToken token = Core.INSTANCE.SettingManager.Settings.MusixMatchTokens[i];

                    if (DateTimeOffset.Now.ToUnixTimeMilliseconds() > token.ExpirationDate)
                    {
                        Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Remove(token);
                        settingsChanged = true;
                    }
                }

                if (settingsChanged) 
                    Core.INSTANCE.SettingManager.WriteSettings();

                await Task.Delay(15000);
            }
        }

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return new LyricData();

            string token = GetRandomMusixMatchToken();

            if (!DataValidator.ValidateData(token))
                return new LyricData();

            MusixmatchClient musixmatchClient = new MusixmatchClient(token);

            if (!DataValidator.ValidateData(musixmatchClient))
                return new LyricData();

            GenericList<Track> tracks = null;

            if (songRequestObject.SelectioMode == SelectionMode.PERFORMANCE)
            {
                tracks = await musixmatchClient.SongSearchAsync(
                    new TrackSearchParameters
                    {
                        Album = songRequestObject.FormattedSongAlbum,
                        Title = songRequestObject.FormattedSongName,
                        Artist = songRequestObject.GetArtistsSplit(),
                        HasSubtitles = true
                    });
            }
            else
            {
                tracks = await musixmatchClient.SongSearchAsync(
                    new TrackSearchParameters
                    {
                        Album = songRequestObject.FormattedSongAlbum,
                        Title = songRequestObject.FormattedSongName,
                        Artist = songRequestObject.GetArtistsSplit(),
                        HasSubtitles = true
                    });

                if (!DataValidator.ValidateData(tracks) || DataValidator.ValidateData(tracks) && tracks.Length == 0)
                {
                    tracks = await musixmatchClient.SongSearchAsync(
                        new TrackSearchParameters
                        {
                            Album = songRequestObject.FormattedSongAlbum,
                            Title = songRequestObject.FormattedSongName,
                            HasSubtitles = true
                        });
                }
            }

            if (!DataValidator.ValidateData(tracks))
            {
                this._debugger.Write("Track not found", DebugType.ERROR);
                return new LyricData();
            }

            this._debugger.Write(string.Format("Found {0} tracks", tracks.Length), DebugType.INFO);

            for (int i = 0; i < tracks.Length; i++)
            {
                Track track = tracks[i];

                if (!IsValidSong(track, songRequestObject))
                    continue;

                if (track.Instrumental == 1)
                {
                    return new LyricData(
                        LyricReturnCode.SUCCESS,
                        SongMetadata.ToSongMetadata(track.TrackName,
                            track.AlbumName,
                            new string[] { track.ArtistName }, 
                            track.TrackLength),
                        LyricType.INSTRUMENTAL);
                }

                if (track.HasSubtitles == 0) 
                    continue;

                try
                {
                    SubtitleRawResponse response = musixmatchClient.GetTrackSubtitlesRaw(track.TrackId, MusixmatchClient.SubtitleFormat.Musixmatch);

                    FileFormatParser<LrcObject> fileFormatParser =
                        new FileFormatParser<LrcObject>(
                            new MmlParser<LrcObject>());

                    if (DataValidator.ValidateData(fileFormatParser))
                    {
                        GenericList<LyricElement> lyricElements =
                            fileFormatParser.FormatFromString(response.SubtitleBody).Lyrics;

                        this._debugger.Write(string.Format("Found lyrics for {0}", track.TrackName), DebugType.INFO);

                        if (DataValidator.ValidateData(lyricElements))
                        {
                            return await LyricData.ConvertToData(
                                lyricElements,
                                SongMetadata.ToSongMetadata(track.TrackName,
                                    track.AlbumName,
                                    new string[] { track.ArtistName },
                                    track.TrackLength),
                                this.CollectorName());
                        }
                    }
                }
                catch (Exception e)
                {
                    this._debugger.Write(e);
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

        private string GetRandomMusixMatchToken()
        {
            if (!DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.MusixMatchTokens))
                return null;

            return Core.INSTANCE.SettingManager.Settings.MusixMatchTokens[new Random().Next(0,
                Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Count - 1)].Token;
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

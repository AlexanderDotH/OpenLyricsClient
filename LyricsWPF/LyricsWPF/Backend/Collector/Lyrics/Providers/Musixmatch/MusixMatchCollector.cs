using System;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.MmlFormat;
using DevBaseFormat.Structure;
using LyricsWPF.Backend.Debug;
using LyricsWPF.Backend.Handler.Song;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Enum;
using LyricsWPF.Backend.Utils;
using MusixmatchClientLib;
using MusixmatchClientLib.API.Contexts;
using MusixmatchClientLib.API.Model.Types;
using MusixmatchClientLib.Auth;
using MusixmatchClientLib.Types;

namespace LyricsWPF.Backend.Collector.Lyrics.Providers.Musixmatch
{
    public class MusixMatchCollector : ICollector
    {
        private Debugger<MusixMatchCollector> _debugger;

        private MusixmatchToken _musixmatchToken;

        private TaskSuspensionToken _collectMusixMatchSuspensionToken;

        public MusixMatchCollector()
        {
            this._debugger = new Debugger<MusixMatchCollector>(this);

            this._musixmatchToken = null;

            if (Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Count > 0)
            {
                this._musixmatchToken = new MusixmatchToken(GetRandomMusixMatchToken(), ApiContext.Desktop);
            }
            else
            {
                try
                {
                    this._musixmatchToken = new MusixmatchToken();

                    MusixMatchToken musixMatchToken = new MusixMatchToken();
                    musixMatchToken.Token = this._musixmatchToken.Token;
                    musixMatchToken.ExpirationDate = DateTimeOffset.Now.AddMinutes(2).ToUnixTimeMilliseconds();

                    Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Add(musixMatchToken);
                    Core.INSTANCE.SettingManager.WriteSettings();
                }
                catch (Exception e) { }
            }

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
                await Task.Delay(15000);

                bool settingsChanged = false;

                try
                {
                    MusixMatchToken musixMatchToken = new MusixMatchToken();
                    musixMatchToken.Token = new MusixmatchToken().Token;
                    musixMatchToken.ExpirationDate = DateTimeOffset.Now.AddMinutes(2).ToUnixTimeMilliseconds();

                    Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Add(musixMatchToken);
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

            }
        }

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return new LyricData(LyricReturnCode.Failed, SongMetadata.ToSongMetadata(songRequestObject));

            if (!DataValidator.ValidateData(this._musixmatchToken))
                return new LyricData(LyricReturnCode.Failed, SongMetadata.ToSongMetadata(songRequestObject));

            MusixmatchClient musixmatchClient = new MusixmatchClient(GetRandomMusixMatchToken());

            if (!DataValidator.ValidateData(musixmatchClient))
                return new LyricData(LyricReturnCode.Failed, SongMetadata.ToSongMetadata(songRequestObject));

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
                return new LyricData(LyricReturnCode.Failed, SongMetadata.ToSongMetadata(songRequestObject));

            for (int i = 0; i < tracks.Length; i++)
            {
                Track track = tracks[i];

                if (!IsValidSong(track, songRequestObject))
                    continue;

                if (track.Instrumental == 1)
                {
                    return new LyricData(
                        LyricReturnCode.Success,
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

        //Untested it should make everything a bit more strict
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

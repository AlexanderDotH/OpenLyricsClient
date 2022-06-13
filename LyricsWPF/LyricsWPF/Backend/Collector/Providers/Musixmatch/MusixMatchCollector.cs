using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Async.Task;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.LrcFormat;
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

namespace LyricsWPF.Backend.Collector.Providers.Musixmatch
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
                    Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Add(this._musixmatchToken.Token);
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

                try
                {
                    Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Add(new MusixmatchToken().Token);
                    Core.INSTANCE.SettingManager.WriteSettings();
                }
                catch (Exception e) { }
            }
        }

        public async Task<LyricData> GetLyrics(SongRequestObject songRequestObject)
        {
            if (!DataValidator.ValidateData(songRequestObject))
                return new LyricData(LyricReturnCode.Failed);

            if (!DataValidator.ValidateData(this._musixmatchToken))
                return new LyricData(LyricReturnCode.Failed);

            MusixmatchClient musixmatchClient = new MusixmatchClient(GetRandomMusixMatchToken());

            if (!DataValidator.ValidateData(musixmatchClient))
                return new LyricData(LyricReturnCode.Failed);

            GenericList<Track> tracks = await musixmatchClient.SongSearchAsync(
                new TrackSearchParameters
            {
                Album = songRequestObject.FormattedSongAlbum, 
                Artist = songRequestObject.GetArtistsSplit(),
                Title = songRequestObject.FormattedSongName,
                HasSubtitles = true,
                Sort = TrackSearchParameters.SortStrategy.TrackRatingAsc
            });

            for (int i = 0; i < tracks.Length; i++)
            {
                Track track = tracks[i];

                if (track.Instrumental == 1)
                {
                    return new LyricData(
                        LyricReturnCode.Success,
                        track.TrackName,
                        track.AlbumName,
                        new string[] { track.ArtistName },
                        LyricType.INSTRUMENTAL);
                }

                SubtitleRawResponse response = await musixmatchClient.GetTrackSubtitlesRawAsync(track.TrackId, MusixmatchClient.SubtitleFormat.Musixmatch);

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
                            track.TrackName,
                            track.AlbumName,
                            new string[] { track.ArtistName }, 
                            this.CollectorName());
                    }
                }
            }

            return null;
        }

        private string GetRandomMusixMatchToken()
        {
            if (!DataValidator.ValidateData(Core.INSTANCE.SettingManager.Settings.MusixMatchTokens))
                return null;

            return Core.INSTANCE.SettingManager.Settings.MusixMatchTokens[new Random().Next(0,
                Core.INSTANCE.SettingManager.Settings.MusixMatchTokens.Count - 1)];
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

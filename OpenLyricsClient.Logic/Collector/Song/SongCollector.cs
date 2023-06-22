using DevBase.Generics;
using OpenLyricsClient.Logic.Collector.Song.Providers.Deezer;
using OpenLyricsClient.Logic.Collector.Song.Providers.Musixmatch;
using OpenLyricsClient.Logic.Collector.Song.Providers.Plugin;
using OpenLyricsClient.Logic.Collector.Song.Providers.Spotify;
using OpenLyricsClient.Logic.Events;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Handler.Artwork;
using OpenLyricsClient.Logic.Handler.Lyrics;
using OpenLyricsClient.Logic.Handler.Song;
using OpenLyricsClient.Shared.Structure.Song;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Collector.Song;

public class SongCollector
{
    private AList<ISongCollector> _songCollectors;
    private ATupleList<SongRequestObject, SongResponseObject> _songResponses;

    private SongHandler _songHandler;
    private LyricHandler _lyricHandler;
    private ArtworkHandler _artworkHandler;

    public SongCollector(SongHandler songHandler, LyricHandler lyricHandler, ArtworkHandler artworkHandler)
    {
        this._songCollectors = new AList<ISongCollector>();
        
        this._songCollectors.Add(new MusixmatchCollector());
        /*this._songCollectors.Add(new NetEaseCollector());
        this._songCollectors.Add(new NetEaseV2Collector());*/
        this._songCollectors.Add(new DeezerSongCollector());
        this._songCollectors.Add(new SpotifyCollector());
        this._songCollectors.Add(new PluginSongCollector());

        this._songResponses = new ATupleList<SongRequestObject, SongResponseObject>();

        this._songHandler = songHandler;

        this._lyricHandler = lyricHandler;
        this._artworkHandler = artworkHandler;
    }

    public async Task FireSongCollector(SongChangedEventArgs songChangedEventArgs)
    {
        if (songChangedEventArgs.EventType == EventType.PRE)
            return;

        SongRequestObject songRequestObject = SongRequestObject.FromSong(songChangedEventArgs.Song);

        for (int i = 0; i < this._songCollectors.Length; i++)
        {
            if (Core.INSTANCE.CacheManager.IsLyricsInCache(songRequestObject) && 
                Core.INSTANCE.CacheManager.IsArtworkInCache(songRequestObject))
                continue;

            ISongCollector songCollector = this._songCollectors.Get(i);
            SongResponseObject songResponseObject = await songCollector.GetSong(songRequestObject);

            if (!(DataValidator.ValidateData(songResponseObject)))
                continue;

            if (!Core.INSTANCE.CacheManager.IsArtworkInCache(songResponseObject.SongRequestObject))
            {
                Task.Factory.StartNew(async () =>
                    await this._artworkHandler.FireArtworkSearch(songResponseObject, songChangedEventArgs));
            }
            
            if (!Core.INSTANCE.CacheManager.IsLyricsInCache(songRequestObject))
            {
                Task.Factory.StartNew(async () =>
                    await this._lyricHandler.FireLyricsSearch(songResponseObject, songChangedEventArgs));
            }
        }
    }
}
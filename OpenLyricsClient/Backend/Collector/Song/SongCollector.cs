using System.Threading.Tasks;
using DevBase.Generic;
using Microsoft.CodeAnalysis.CSharp;
using OpenLyricsClient.Backend.Collector.Song.Providers.Deezer;
using OpenLyricsClient.Backend.Collector.Song.Providers.Musixmatch;
using OpenLyricsClient.Backend.Collector.Song.Providers.NetEase;
using OpenLyricsClient.Backend.Collector.Song.Providers.NetEaseV2;
using OpenLyricsClient.Backend.Events;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Handler.Artwork;
using OpenLyricsClient.Backend.Handler.Lyrics;
using OpenLyricsClient.Backend.Handler.Song;
using OpenLyricsClient.Backend.Structure.Song;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Collector.Song;

public class SongCollector
{
    private GenericList<ISongCollector> _songCollectors;
    private GenericTupleList<SongRequestObject,SongResponseObject> _songResponses;

    private SongHandler _songHandler;
    private LyricHandler _lyricHandler;
    private ArtworkHandler _artworkHandler;
    
    public SongCollector(SongHandler songHandler, LyricHandler lyricHandler, ArtworkHandler artworkHandler)
    {
        this._songCollectors = new GenericList<ISongCollector>();
        this._songCollectors.Add(new MusixmatchCollector());
        this._songCollectors.Add(new NetEaseCollector());
        this._songCollectors.Add(new NetEaseV2Collector());
        this._songCollectors.Add(new DeezerSongCollector());

        this._songResponses = new GenericTupleList<SongRequestObject, SongResponseObject>();
        
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
            ISongCollector songCollector = this._songCollectors.Get(i);
            SongResponseObject songResponseObject = await songCollector.GetSong(songRequestObject);

            if (!(DataValidator.ValidateData(songResponseObject)))
                continue;
            
            if (DataValidator.ValidateData(songResponseObject))
            {
                Task.Factory.StartNew(async () =>
                {
                    await this._lyricHandler.FireLyricsSearch(songResponseObject, songChangedEventArgs);
                });
                Task.Factory.StartNew(async () =>
                {
                    await this._artworkHandler.FireArtworkSearch(songResponseObject, songChangedEventArgs);
                });
            }
        }
    }


}
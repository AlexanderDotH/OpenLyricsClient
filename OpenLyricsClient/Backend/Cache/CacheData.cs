using System;
using OpenLyricsClient.Shared.Structure.Artwork;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Backend.Cache;

[Serializable]
public class CacheData
{
    private SongMetadata _songMetadata;
    private LyricData _lyricData;
    private Artwork _artwork;

    public CacheData(SongMetadata songMetadata, LyricData lyricData, Artwork artwork)
    {
        _songMetadata = songMetadata;
        _lyricData = lyricData;
        _artwork = artwork;
    }

    public SongMetadata SongMetadata
    {
        get => _songMetadata;
        set => _songMetadata = value;
    }

    public LyricData LyricData
    {
        get => _lyricData;
        set => _lyricData = value;
    }

    public Artwork Artwork
    {
        get => _artwork;
        set => _artwork = value;
    }
}
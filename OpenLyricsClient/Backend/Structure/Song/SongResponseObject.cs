using System;

namespace OpenLyricsClient.Backend.Structure.Song;

public class SongResponseObject
{
    private SongRequestObject _songRequestObject;
    private Object _track;
    private string _collectorName;

    public SongResponseObject() { }

    public SongResponseObject(SongRequestObject songRequestObject, Object track, string collectorName)
    {
        _songRequestObject = songRequestObject;
        _track = track;
        _collectorName = collectorName;
    }

    public SongRequestObject SongRequestObject
    {
        get => _songRequestObject;
        set => _songRequestObject = value;
    }

    public object Track
    {
        get => _track;
        set => _track = value;
    }

    public string CollectorName
    {
        get => _collectorName;
        set => _collectorName = value;
    }
}
using DevBase.Generics;
using SpotifyAPI.Web;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Shared.Structure.Other;

public class SimpleTrack
{
    public string Name { get; set; }
    public string Album { get; set; }
    public string[] Artists { get; set; }
    public Image[] Cover { get; set; }

    public static SimpleTrack ConvertTo(FullTrack fullTrack)
    {
        AList<string> artists = new AList<string>();
        fullTrack.Artists.ForEach(artist => artists.Add(artist.Name));

        SimpleTrack simpleTrack = new SimpleTrack
        {
            Name = fullTrack.Name,
            Album = fullTrack.Album.Name,
            Artists = artists.GetAsArray(),
            Cover = fullTrack.Album.Images.ToArray()
        };

        return simpleTrack;
    }
    
    public static SimpleTrack[] ConvertTo(AList<FullTrack> fullTrack)
    {
        AList<SimpleTrack> tracks = new AList<SimpleTrack>();
        fullTrack.ForEach(track => tracks.Add(ConvertTo(track)));
        return tracks.GetAsArray();
    }

    
}
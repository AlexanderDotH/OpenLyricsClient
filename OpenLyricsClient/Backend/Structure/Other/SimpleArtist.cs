using DevBase.Generics;
using SpotifyAPI.Web;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Backend.Structure.Other;

public class SimpleArtist
{
    public string Name { get; set; }
    public Image[] Images { get; set; }
    public string[] Genres { get; set; }
    public int Popularity { get; set; }

    public static SimpleArtist ConvertTo(FullArtist fullArtist)
    {
        SimpleArtist artist = new SimpleArtist
        {
            Name = fullArtist.Name,
            Images = fullArtist.Images.ToArray(),
            Genres = fullArtist.Genres.ToArray(),
            Popularity = fullArtist.Popularity
        };

        return artist;
    }
    
    public static SimpleArtist[] ConvertTo(AList<FullArtist> fullArtists)
    {
        AList<SimpleArtist> artists = new AList<SimpleArtist>();

        fullArtists.ForEach(artist => artists.Add(ConvertTo(artist)));

        return artists.GetAsArray();
    }
    
}
using Avalonia.Media;
using DevBase.Avalonia.Color.Extensions;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Shared.Structure.Palette;

namespace OpenLyricsClient.Backend.Handler.ColorHandler;

public class ColorHandler
{
    public ColorHandler()
    {
        Core.INSTANCE.ArtworkHandler.ArtworkFoundHandler += ArtworkHandlerOnArtworkFoundHandler;
    }

    private void ArtworkHandlerOnArtworkFoundHandler(object sender, ArtworkFoundEventArgs args)
    {
        UpdateColor(args.Artwork.ArtworkColor);
    }

    private void UpdateColor(Color color)
    {
        ColorPalette palette = new ColorPalette
        {
            /*PrimaryColor = new ColorPair
            {
                color.Correct()
            }*/
        };


    }
}
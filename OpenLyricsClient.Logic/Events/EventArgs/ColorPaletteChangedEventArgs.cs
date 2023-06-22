using OpenLyricsClient.Shared.Structure.Palette;

namespace OpenLyricsClient.Logic.Events.EventArgs;

public class ColorPaletteChangedEventArgs : System.EventArgs
{
    private ColorPalette _colorPalette;

    public ColorPaletteChangedEventArgs(ColorPalette palette)
    {
        this._colorPalette = palette;
    }

    public ColorPalette ColorPalette
    {
        get => this._colorPalette;
    }
}
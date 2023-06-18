using Avalonia.Media;

namespace OpenLyricsClient.Shared.Structure.Palette;

public class ColorPalette
{
    public ColorPair PrimaryColor { get; set; }
    public ColorPair SecondaryColor { get; set; }
    public ColorPair SelectedLineColor { get; set; }
    public ColorPair UnSelectedLineColor { get; set; }
    public ColorPair PrimaryForegroundColor { get; set; }
    public ColorPair SecondaryForegroundColor { get; set; }
}
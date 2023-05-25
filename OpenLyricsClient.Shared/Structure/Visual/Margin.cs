using Avalonia;

namespace OpenLyricsClient.Shared.Structure.Visual;

// I can´t simply use the Thicknes type because it can´t be properly deserialized from Newtonsoft.Json
public class Margin
{
    public double Left { get; set; }
    public double Top { get; set; }
    public double Right { get; set; }
    public double Bottom { get; set; }

    public Margin(double left, double top, double right, double bottom)
    {
        this.Left = left;
        this.Top = top;
        this.Right = right;
        this.Bottom = bottom;
    }
    
    public Thickness ToThickness() => new Thickness(Left, Top, Right, Bottom);
}
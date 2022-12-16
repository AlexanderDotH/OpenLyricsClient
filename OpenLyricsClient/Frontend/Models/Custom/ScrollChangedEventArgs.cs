using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace OpenLyricsClient.Frontend.Models.Custom;

public class ScrollChangedEventArgs : RoutedEventArgs
{
    public ScrollChangedEventArgs(
        Vector extentDelta,
        Vector offsetDelta,
        Vector viewportDelta)
        : this(CustomScrollViewer.ScrollChangedEvent, extentDelta, offsetDelta, viewportDelta)
    {
    }

    public ScrollChangedEventArgs(
        RoutedEvent routedEvent,
        Vector extentDelta,
        Vector offsetDelta,
        Vector viewportDelta)
        : base(routedEvent)
    {
        ExtentDelta = extentDelta;
        OffsetDelta = offsetDelta;
        ViewportDelta = viewportDelta;
    }

    /// <summary>
    /// Gets the change to the value of <see cref="ScrollViewer.Extent"/>.
    /// </summary>
    public Vector ExtentDelta { get; }

    /// <summary>
    /// Gets the change to the value of <see cref="ScrollViewer.Offset"/>.
    /// </summary>
    public Vector OffsetDelta { get; }

    /// <summary>
    /// Gets the change to the value of <see cref="ScrollViewer.Viewport"/>.
    /// </summary>
    public Vector ViewportDelta { get; }
}
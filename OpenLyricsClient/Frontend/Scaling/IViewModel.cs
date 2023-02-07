namespace OpenLyricsClient.Frontend.Scaling;

/// <summary>
/// Indicates that a class acts as a view model for a corresponding <see cref="Window"/>.
/// </summary>
public interface IViewModel
{
    public bool IsResizing { get; set; }
}
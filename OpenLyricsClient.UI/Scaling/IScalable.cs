using System;

namespace OpenLyricsClient.UI.Scaling;

/// <summary>
/// Represents any UI component that supports UI scaling.
/// </summary>
public interface IScalable : IDisposable
{
    /// <summary>
    /// Applies the specified <paramref name="scalingFactor"/> to this <see cref="IScalable"/>.
    /// </summary>
    /// <param name="scalingFactor">The scaling factor to be applied to this <see cref="IScalable"/>.</param>
    public void ApplyScaling(double scalingFactor);
}
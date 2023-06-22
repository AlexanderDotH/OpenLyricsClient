using System;
using System.Collections.Generic;

namespace OpenLyricsClient.UI.Scaling;

/// <summary>
/// Provides access to all instances of the <see cref="ScalingManager"/>.
/// </summary>
public static class ScalingProvider
{
    private static readonly Dictionary<Type, ScalingManager> scalingManagers = new Dictionary<Type, ScalingManager>();

    /// <summary>
    /// Retrieves the <see cref="ScalingManager"/> linked to the <see cref="Type"/> of <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of the <see cref="ScalableWindow"/> to get the instance of the <see cref="ScalingManager"/> for.</typeparam>
    public static ScalingManager GetInstance<T>() where T : ScalableWindow
    {
        return GetInstance(typeof(T));
    }

    /// <summary>
    /// Retrieves the <see cref="ScalingManager"/> linked to the <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type of the <see cref="ScalableWindow"/> to get the instance of the <see cref="ScalingManager"/> for.</param>
    public static ScalingManager GetInstance(Type type)
    {
        return scalingManagers.GetValueOrDefault(type);
    }

    /// <summary>
    /// Creates a new <see cref="ScalingManager"/> and sets up all bindings of the <paramref name="window"/>.
    /// </summary>
    /// <param name="window">The <see cref="ScalableWindow"/> to be managed by this <see cref="ScalingManager"/>.</param>
    /// <param name="viewModel">The <see cref="IViewModel"/> implemented by the view model belonging to the <paramref name="window"/>.</param>
    public static ScalingManager Register(ScalableWindow window, IViewModel viewModel)
    {
        Type type = window.GetType();
        ScalingManager scalingManager = new ScalingManager(window, viewModel);

        if (!scalingManagers.ContainsKey(type))
            scalingManagers.Add(type, scalingManager);

        return scalingManager;
    }
}
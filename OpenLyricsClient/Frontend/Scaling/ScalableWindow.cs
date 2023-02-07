using System;
using Avalonia.Controls;
using Avalonia.Input;

namespace OpenLyricsClient.Frontend.Scaling;

public abstract class ScalableWindow : Window
{
    public abstract event EventHandler<PointerPressedEventArgs> BeginResize;
    public abstract event EventHandler<PointerEventArgs> Resize;
    public abstract event EventHandler<PointerReleasedEventArgs> EndResize;
}
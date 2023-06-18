using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Extensions;
using OpenLyricsClient.Shared.Structure.Lyrics;

namespace OpenLyricsClient.Frontend.Structure;

public class LyricOverlayElement : INotifyPropertyChanged
{
    private double _width;

    public LyricOverlayElement() { }

    public LyricOverlayElement(string line, Rect rect)
    {
        this.Line = line;
        this.Rect = rect;
    }
    
    public Rect Rect { get; set; }
    
    public string Line { get; set; }

    public double Width
    {
        get => this._width;
        set
        {
            SetField(ref this._width, value);
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
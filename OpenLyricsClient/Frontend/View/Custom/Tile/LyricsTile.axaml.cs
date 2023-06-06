using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Models.Pages.Settings;
using OpenLyricsClient.Frontend.View.Custom.Tile.Overlays;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Visual;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Frontend.View.Custom.Tile;

public partial class LyricsTile : UserControl, INotifyPropertyChanged
{
    public static readonly DirectProperty<LyricsTile, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<LyricsTile, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private LyricPart _lyricPart;
    private Decorator _decorator;

    private Thickness _lyricsMargin;

    private UserControl _overlay;

    private double _speed;
    
    public LyricsTile()
    {
        AvaloniaXamlLoader.Load(this);

        this._decorator = this.Get<Decorator>(nameof(PART_Decorator));

        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        Margin t = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<Margin>("Lyrics Margin");
        this._decorator.Margin = t.ToThickness();
    }

    public void UpdateViewPort(double width, double height)
    {
        //this._overlay.UpdateViewPort(width, height);
    }
    
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs args)
    {
        //Dispatcher.UIThread.InvokeAsync(() => this._overlay.UpdateViewPort(this.Width, this.Height));
    }
    
    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        
    }
    
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

    public Size Size
    {
        get
        {
            Size s = new Size();
            
            if (this._overlay is NoteOverlay overlay)
                s = overlay.Size;
            
            if (this._overlay is TextOverlay text)
                s = text.Size;    
            
            Thickness t = this._decorator.Margin;

            return new Size(
                s.Width + t.Right + t.Left,
                s.Height + t.Top + t.Bottom);
        }
    }
    
    public LyricPart LyricPart
    {
        get { return this._lyricPart; }
        set
        {
            if (!DataValidator.ValidateData(value))
                return;
            
            ApplyDataToOverlay(value);
            this._decorator.Child = this._overlay;
            
            SetAndRaise(LyricPartProperty, ref _lyricPart, value);
        }
    }
    
    private void ApplyDataToOverlay(LyricPart lyricPart)
    {
        if (lyricPart.Part.Contains("♪"))
        {
            this._overlay = new NoteOverlay();
            (this._overlay as NoteOverlay).LyricPart = lyricPart;
        } 
        else if (!lyricPart.Part.Contains("♪"))
        {
            this._overlay = new TextOverlay();
            (this._overlay as TextOverlay).LyricPart = lyricPart;
        }

    }
}
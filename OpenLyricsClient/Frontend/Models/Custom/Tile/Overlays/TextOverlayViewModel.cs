using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using DevBase.Generics;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Utils;

namespace OpenLyricsClient.Frontend.Models.Custom.Tile.Overlays;

public class TextOverlayViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private AList<string> _lines;

    public TextOverlayViewModel()
    {
        this._lines = new AList<string>();
        
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
    }

    public void UpdateTextWrappingLines(string text, double width, double height)
    {
        AList<string> lines = StringUtils.SplitTextToLines(
            text,
            width,
            height,
            new Typeface(FontFamily.Parse(
                    "avares://Material.Styles/Fonts/Roboto#Roboto"), 
                FontStyle.Normal, this.LyricsWeight),
            this.LyricsAlignment,
            this.LyricsSize);

        SetField(ref this._lines, lines);
    }
    
    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        OnPropertyChanged("LyricsSize");
        OnPropertyChanged("LyricsWeight");
        OnPropertyChanged("LyricsAlignment");
        OnPropertyChanged("LyricsMargin");
    }

    public double LyricsSize
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<double>("Lyrics Size");
    }
    
    public FontWeight LyricsWeight 
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<FontWeight>("Lyrics Weight");
    }
    
    public TextAlignment LyricsAlignment 
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<TextAlignment>("Lyrics Alignment");
    }
    
    public Thickness LyricsMargin 
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<Thickness>("Lyrics Margin");
    }

    public AList<string> LyricsLines
    {
        get => this._lines;
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
}
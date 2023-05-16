using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia;
using Avalonia.Layout;
using Avalonia.Media;
using DevBase.Extensions;
using DevBase.Generics;
using Microsoft.CodeAnalysis;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Frontend.Structure;
using OpenLyricsClient.Frontend.Utils;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;
using Org.BouncyCastle.Crypto.Parameters;
using ReactiveUI;
using SharpDX;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.Frontend.Models.Custom.Tile.Overlays;

public class TextOverlayViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private ObservableCollection<LyricOverlayElement> _lines;
    private LyricPart _lyricPart;
    private Typeface _typeface;

    public ICommand EffectiveViewportChangedCommand { get; }
    
    public TextOverlayViewModel()
    {
        this._lines = new ObservableCollection<LyricOverlayElement>();
        
        this._typeface = new Typeface(FontFamily.Parse(
                "avares://Material.Styles/Fonts/Roboto#Roboto"),
            FontStyle.Normal, this.LyricsWeight);
        
        EffectiveViewportChangedCommand = ReactiveCommand.Create<EffectiveViewportChangedEventArgs>(OnEffectiveViewportChanged);
        
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
    }

    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (!args.LyricPart.Equals(this.LyricPart))
            return;
        
        UpdatePercentage(this.LyricPart.Part);
        OnPropertyChanged("LyricsLines");
    }

    private void OnEffectiveViewportChanged(EffectiveViewportChangedEventArgs e)
    {
        //UpdateLyricsWrapping(e.EffectiveViewport.Width, e.EffectiveViewport.Height);   
    }

    public void UpdateLyricsWrapping(double width, double height)
    {
        if (!DataValidator.ValidateData(this.LyricPart))
            return;
        
        UpdateTextWrappingLines(this.LyricPart.Part, width, height);
    }

    private void UpdateTextWrappingLines(string text, double width, double height)
    {
        AList<string> lines = StringUtils.SplitTextToLines(
            text,
            width,
            height,
            this._typeface,
            this.LyricsAlignment,
            this.LyricsSize);

        ObservableCollection<LyricOverlayElement> sizedLines = new ObservableCollection<LyricOverlayElement>();
        
        lines.ForEach(l =>
        {
            LyricOverlayElement element = new LyricOverlayElement
            {
                Rect = MeasureSingleString(l),
                Percentage = CalculatePercentage(l, text),
                Line = l
            };
            sizedLines.Add(element);
        });

        SetField(ref this._lines, sizedLines);
    }

    private void UpdatePercentage(string text)
    {
        this._lines.ForEach(l =>
        {
            l.Percentage = CalculatePercentage(l.Line, text);
            l.Line = l.Line + " " + l.Percentage.ToString();
        });
    }

    private double CalculatePercentage(string single, string full)
    {
        double singleWidth = MeasureSingleString(single).Width;
        double fullWidth = MeasureSingleString(full).Width;

        return (fullWidth * 0.01) * singleWidth;
    }

    private Rect MeasureSingleString(string line, TextWrapping wrapping = TextWrapping.NoWrap)
    {
        FormattedText formattedCandidateLine = new FormattedText(
            line, 
            this._typeface, 
            this.LyricsSize, 
            this.LyricsAlignment, 
            wrapping, 
            new Size(double.PositiveInfinity, double.PositiveInfinity));

        return formattedCandidateLine.Bounds;
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

    public LyricPart LyricPart
    {
        get => this._lyricPart;
        set
        {
            SetField(ref this._lyricPart, value);
        }
    }

    public ObservableCollection<LyricOverlayElement> LyricsLines
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
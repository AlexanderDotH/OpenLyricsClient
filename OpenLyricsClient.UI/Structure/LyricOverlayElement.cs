using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using Accord.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.UI.Extensions;
using OpenLyricsClient.UI.Models;
using OpenLyricsClient.UI.View.Custom.Tile.Overlays;

namespace OpenLyricsClient.UI.Structure;

public class LyricOverlayElement : ModelBase
{
    private double _width;
    private double _prevWidth;
    
    private double _percentage;
    private double _prevPercentage;
    
    private bool _selected;
    
    private double _percentageMargin;
    private double _prevPercentageMargin;

    private SolidColorBrush _primaryThemeColorBrush;
    private SolidColorBrush _selectedLineFontColorBrush;
    private SolidColorBrush _unSelectedLineFontColorBrush;

    private SolidColorBrush _unSelectedLineColorBrush;

    private bool _pointerOver;

    public LyricOverlayElement()
    {
        this._selected = false;

        this._prevPercentage = 0;
        this._prevWidth = 0;
        this._prevPercentageMargin = 0;
    }

    private void LocateResource()
    {
        if (this._primaryThemeColorBrush == null ||
            this._selectedLineFontColorBrush == null ||
            this._unSelectedLineFontColorBrush == null ||
            this._unSelectedLineColorBrush == null)
        {
            this._primaryThemeColorBrush = App.Current.FindResource("PrimaryThemeColorBrush") as SolidColorBrush;
            this._selectedLineFontColorBrush = App.Current.FindResource("SelectedLineFontColorBrush") as SolidColorBrush;
            this._unSelectedLineFontColorBrush = App.Current.FindResource("UnSelectedLineFontColorBrush") as SolidColorBrush;

            this._unSelectedLineColorBrush = SolidColorBrush.Parse("#646464");
        }
    }

    public LyricOverlayElement(string line, Size rect) : this()
    {
        this.Line = line;
        this.Rect = rect;
    }
    
    public Size Rect { get; set; }
    
    public string Line { get; set; }

    public bool PointerOver
    {
        get => this._pointerOver;
        set
        {
            this._pointerOver = value;

            OnPropertyChanged("FadeSelectedColor");
            OnPropertyChanged("SolidSelectedColor");
            OnPropertyChanged("UnSelectedColor");
        }
    }

    public bool Selected
    {
        get => _selected;
        set
        {
            this._selected = value;
            
            /*
            OnPropertyChanged("FadeSelectedColor");
            OnPropertyChanged("SolidSelectedColor");
            OnPropertyChanged("UnSelectedColor");*/
        }
    }

    public double Width
    {
        get => this._width;
        set
        {
            if (value == this._prevWidth)
                return;
            
            SetField(ref this._width, value);

            this._prevWidth = value;
        }
    }

    public double Percentage
    {
        get => this._percentage;
        set
        {
            SetField(ref this._percentage, value);
            
            if (value == this._prevPercentage)
                return;
            
            OnPropertyChanged("FadeSelectedColor");
            OnPropertyChanged("SolidSelectedColor");
            OnPropertyChanged("UnSelectedColor");

            this._prevPercentage = value;
        }
    }
    
    public double PercentageMargin
    {
        get => this._percentageMargin;
        set
        {
            if (value == this._percentageMargin)
                return;
            
            SetField(ref this._percentageMargin, value);

            this._prevPercentageMargin = value;
        }
    }

    public SolidColorBrush FadeSelectedColor
    {
        get
        {
            LocateResource();
            
            EnumLyricsDisplayMode displayMode = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()
                .GetValue<EnumLyricsDisplayMode>("Selection Mode");

            if (displayMode != EnumLyricsDisplayMode.FADE)
                return null;
            
            if (!this._selected)
                return UnSelectedColor;
            
            SolidColorBrush colorBrush = this._primaryThemeColorBrush;
            bool artworkBackground =
                Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background");
            
            if (artworkBackground)
                colorBrush = this._selectedLineFontColorBrush;

            if (this._percentage < this._percentageMargin)
                colorBrush = UnSelectedColor.InterpolateTo(colorBrush, _percentage);
            
            if (this._pointerOver)
                colorBrush = colorBrush.AdjustBrightness(90);

            if (artworkBackground && this._pointerOver)
                colorBrush = colorBrush.AdjustBrightness(120);
            
            return colorBrush;
        }
    }
    
    public SolidColorBrush SolidSelectedColor
    {
        get
        {
            LocateResource();

            if (!this._selected)
                return UnSelectedColor;
            
            SolidColorBrush colorBrush = this._primaryThemeColorBrush;
            bool artworkBackground =
                Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background");

            if (artworkBackground)
                colorBrush = this._selectedLineFontColorBrush;
            
            if (this._pointerOver)
                colorBrush = colorBrush.AdjustBrightness(90);

            if (artworkBackground && this._pointerOver)
                colorBrush = colorBrush.AdjustBrightness(120);
            
            return colorBrush;
        }
    }

    public SolidColorBrush UnSelectedColor
    {
        get
        {
            LocateResource();

            SolidColorBrush colorBrush = this._unSelectedLineColorBrush;
            bool artworkBackground =
                Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Artwork Background");
            
            if (artworkBackground)
                colorBrush = this._unSelectedLineFontColorBrush;

            if (this._pointerOver)
                colorBrush = colorBrush.AdjustBrightness(90);

            if (artworkBackground && this._pointerOver)
                colorBrush = colorBrush.AdjustBrightness(120);
            
            return colorBrush;
        }
    }
}
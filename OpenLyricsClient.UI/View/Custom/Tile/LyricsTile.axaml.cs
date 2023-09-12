﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Logic.Utils;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Visual;
using OpenLyricsClient.Shared.Utils;
using OpenLyricsClient.UI.Models.Custom;
using OpenLyricsClient.UI.Models.Elements.Blur;
using OpenLyricsClient.UI.View.Custom.Tile.Overlays;
using Squalr.Engine.Utils.Extensions;

namespace OpenLyricsClient.UI.View.Custom.Tile;

public partial class LyricsTile : UserControl, INotifyPropertyChanged
{
    public static readonly DirectProperty<LyricsTile, LyricPart> LyricPartProperty = 
        AvaloniaProperty.RegisterDirect<LyricsTile, LyricPart>(nameof(LyricPart), o => o.LyricPart, (o, v) => o.LyricPart = v);
    
    public static readonly DirectProperty<LyricsTile, bool> HeadlessProperty = 
        AvaloniaProperty.RegisterDirect<LyricsTile, bool>(nameof(Headless), o => o.Headless, (o, v) => o.Headless = v);
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    private LyricPart _lyricPart;
    private Decorator _decorator;
    private TextBlock _debugBlock;
    private Border _debugBorder;
    private BlurArea _blur;
    
    private Thickness _lyricsMargin;

    private UserControl _overlay;
    
    private double _speed;

    private bool _headless;
    private bool _prevBlurSetting;

    private EnumElementType _elementType;
    
    public LyricsTile()
    {
        AvaloniaXamlLoader.Load(this);

        this._decorator = this.Get<Decorator>(nameof(PART_Decorator));
        /*this._debugBlock = this.Get<TextBlock>(nameof(PART_Debug_Text));
        this._debugBorder = this.Get<Border>(nameof(PART_Debug_Border));*/
        this._blur = this.Get<BlurArea>(nameof(PART_Blur));
        //this._blur.IsVisible = false;

        this._prevBlurSetting = false;
        
        if (Core.DEBUG_MODE)
        {
            this._debugBlock.IsVisible = true;
            this._debugBorder.IsVisible = true;
        }
        
        Core.INSTANCE.LyricHandler.LyricsPercentageUpdated += LyricHandlerOnLyricsPercentageUpdated;
        Core.INSTANCE.LyricHandler.LyricsFound += LyricHandlerOnLyricsFound;
        Core.INSTANCE.LyricHandler.LyricTimeChanged += LyricHandlerOnLyricChanged;
        
        Core.INSTANCE.SettingsHandler.SettingsChanged += SettingsHandlerOnSettingsChanged;
    }

    private void LyricHandlerOnLyricChanged(object sender, LyricChangedEventArgs lyricchangedeventargs)
    {
        bool isBlurEnabled =
            Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Blur Lyrics");

        if (isBlurEnabled != this._prevBlurSetting)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                this._blur.IsVisible = isBlurEnabled;
            });

            this._prevBlurSetting = isBlurEnabled;
        }
        
        if (!isBlurEnabled)
            return;
        
        ApplyBlur(lyricchangedeventargs.LyricPart);
    }

    private void ApplyBlur(LyricPart part)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            int pos1 = GetPositionOfElement(part);
            int pos2 = GetPositionOfElement(this._lyricPart);

            float size = Math.Abs(pos1 - pos2) / 1.5f;
            
            bool isBlurEnabled =
                Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()!.GetValue<bool>("Blur Lyrics");
            
            this._blur.IsVisible = LyricsScroller.Instance.IsSynced && 
                                   LyricsScroller.Instance.IsScrollerReady &&
                                   size < 10 && size >= 0 &&
                                   isBlurEnabled;
            
            this._blur.Sigma = size;
        });
    }

    private int GetPositionOfElement(LyricPart part)
    {
        LyricsScroller parent = LyricsScroller.Instance;
        LyricsScrollerViewModel model = parent.DataContext as LyricsScrollerViewModel;

        for (int i = 0; i < model.Lyrics.Count; i++)
        {
            if (model.Lyrics[i].Equals(part))
                return i;
        }

        return -1;
    }

    private LyricPart GetCurrentElement()
    {
        LyricsScroller parent = LyricsScroller.Instance;
        LyricsScrollerViewModel model = parent.DataContext as LyricsScrollerViewModel;
        return model.Lyric;
    }

    private void SettingsHandlerOnSettingsChanged(object sender, SettingsChangedEventArgs settingschangedeventargs)
    {
        Margin t = Core.INSTANCE.SettingsHandler.Settings<LyricsSection>().GetValue<Margin>("Lyrics Margin");
        this._decorator.Margin = t.ToThickness();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    private void LyricHandlerOnLyricsFound(object sender, LyricsFoundEventArgs args)
    {
        /*Dispatcher.UIThread.InvokeAsync(() =>
        {
            ApplyBlur(this.GetCurrentElement());
        });*/
    }
    
    private void LyricHandlerOnLyricsPercentageUpdated(object sender, LyricsPercentageUpdatedEventArgs args)
    {
        if (Core.DEBUG_MODE)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (this._elementType == EnumElementType.NOTE)
                {
                    this._debugBlock.Text = string.Format(
                        "Type: NoteElement | Data: {0}% | Dimensions: {1}w : {2}h", 
                        LyricPart.Percentage, this.Size.Width, this.Size.Height);
                } 
                else if (this._elementType == EnumElementType.TEXT)
                {
                    this._debugBlock.Text = string.Format("Type: TextElement | Data: {0} {1}% | Dimensions: {2}w : {3}h", 
                        LyricPart.Part, LyricPart.Percentage, this.Size.Width, this.Size.Height);
                }
            });
        }
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
            /*this.InitializeIfNeeded();
            this.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            return this.DesiredSize;*/
            //return this.MeasureOverride(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Size s = new Size();
            
            if (this._overlay is NoteOverlay overlay)
                s = overlay.Size;

            if (this._overlay is TextOverlay text)
            {
                s = new Size(text.Size.Width, text.Size.Height);
            }
            
            Thickness t = this._decorator.Margin;

            double width = s.Width + t.Right + t.Left;
            double height = s.Height + t.Top + t.Bottom;

            /*width = Math.Ceiling(width);
            height = Math.Ceiling(height);*/

            return new Size(
                width,
                height);
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

    public bool Headless
    {
        get { return this._headless; }
        set
        {
            SetAndRaise(HeadlessProperty, ref _headless, value);

            if (this._overlay is NoteOverlay overlay)
                overlay.Headless = value;
            
            if (this._overlay is TextOverlay text)
                text.Headless = value;
        }
    }

    private void ApplyDataToOverlay(LyricPart lyricPart)
    {
        EnumElementType type = TypeClassifier.ClassifyLyricPart(lyricPart);

        switch (type)
        {
            case EnumElementType.NOTE:
            {
                this._overlay = new NoteOverlay()
                {
                    LyricPart = lyricPart
                };
                break;
            }
            case EnumElementType.TEXT:
            {
                this._overlay = new TextOverlay()
                {
                    LyricPart = lyricPart
                };
                break;
            }
        }

        this._elementType = type;
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        this._overlay = null;
        this._lyricPart = null;
        base.OnDetachedFromLogicalTree(e);
    }

    public EnumElementType ElementType
    {
        get => _elementType;
    }
}
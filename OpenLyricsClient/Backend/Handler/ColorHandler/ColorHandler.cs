using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Avalonia.Color.Extensions;
using DevBase.Avalonia.Extension.Color.Image;
using DevBase.Avalonia.Extension.Converter;
using DevBase.Avalonia.Extension.Extension;
using DevBase.Generics;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Shared.Structure.Palette;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Backend.Handler.ColorHandler;

using Artwork = Shared.Structure.Artwork.Artwork;

public class ColorHandler
{
    #region Resources

    // I don´t think its needed in this case
    private SolidColorBrush _primaryColorBrush;
    
    // The dynamic ones
    private SolidColorBrush _primaryThemeColorBrush;
    private SolidColorBrush _primaryThemeFontColorBrush;

    private SolidColorBrush _secondaryThemeColorBrush;
    private SolidColorBrush _secondaryThemeFontColorBrush;
    
    private SolidColorBrush _lightThemeFontColorBrush;
    private SolidColorBrush _selectedLineFontColorBrush;
    private SolidColorBrush _unselectedLineFontColorBrush;
    
    #endregion

    private Color _defaultColor;

    private readonly double _lightAdjustment = 80d;
    
    public ColorHandler()
    {
        this._defaultColor = new Color(255, 220, 20, 60);

        this._primaryColorBrush = FromResource("PrimaryColorBrush");

        this._primaryThemeColorBrush = FromResource("PrimaryThemeColorBrush");
        this._primaryThemeFontColorBrush = FromResource("PrimaryThemeFontColorBrush");

        this._secondaryThemeColorBrush = FromResource("SecondaryThemeColorBrush");
        this._secondaryThemeFontColorBrush = FromResource("SecondaryThemeFontColorBrush");
        
        this._lightThemeFontColorBrush = FromResource("LightThemeFontColorBrush");
        this._selectedLineFontColorBrush = FromResource("SelectedLineFontColorBrush");
        this._unselectedLineFontColorBrush = FromResource("UnSelectedLineFontColorBrush");

        Core.INSTANCE.ArtworkHandler.ArtworkFoundHandler += ArtworkHandlerOnArtworkFoundHandler;
    }

    private void ArtworkHandlerOnArtworkFoundHandler(object sender, ArtworkFoundEventArgs args)
    {
        Task.Factory.StartNew(async () =>
        {
            await UpdateColorAndApply(args.Artwork, args.SongRequestObject);
        });
    }

    private async Task UpdateColorAndApply(Artwork artwork, SongRequestObject requestObject)
    {
        if (artwork.ArtworkCalculated)
        {
            ApplyColor(artwork);
            return;
        }

        Color color = GetColor(artwork);

        ColorPalette palette = new ColorPalette();
        
        ColorPair primary = new ColorPair();
        ColorPair secondary = new ColorPair();
        ColorPair selected = new ColorPair();
        ColorPair unselected = new ColorPair();
        ColorPair primaryForeground = new ColorPair();
        ColorPair secondaryForeground = new ColorPair();
        
        primary.Dark = color;
        primary.Light = color.AdjustBrightness(this._lightAdjustment);

        secondary.Dark = color.AdjustBrightness(90d);
        secondary.Light = color.AdjustBrightness(this._lightAdjustment);

        selected.Dark = color.AdjustBrightness(120);
        selected.Light = color.AdjustBrightness(60);

        unselected.Dark = color.AdjustBrightness(90d);
        unselected.Light = color.AdjustBrightness(this._lightAdjustment);
        
        primaryForeground.Dark = new Color(255, 255, 255, 255);
        primaryForeground.Light = new Color(255, 22, 22, 22);

        secondaryForeground.Dark = new Color(255,255,255,255).AdjustBrightness(90d);
        secondaryForeground.Light = new Color(255,22, 22, 22).AdjustBrightness(110d);
        
        palette.PrimaryColor = primary;
        palette.SecondaryColor = secondary;
        palette.SelectedLineColor = selected;
        palette.UnSelectedLineColor = unselected;
        palette.PrimaryForegroundColor = primaryForeground;
        palette.SecondaryForegroundColor = secondaryForeground;

        artwork.ArtworkColor = palette;
        artwork.ArtworkCalculated = true;
        
        ApplyColor(artwork);
    }

    private async Task ApplyColor(Artwork artwork)
    {
        if (artwork.ArtworkApplied)
            return;
        
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ColorPalette palette = artwork.ArtworkColor;

            UpdateColor(this._primaryThemeColorBrush, palette.PrimaryColor.Dark);
            UpdateColor(this._secondaryThemeColorBrush, palette.PrimaryColor.Dark);

            if (palette.PrimaryColor.Dark.BrightnessPercentage() > 30)
            {
                UpdateColor(this._primaryThemeFontColorBrush, palette.PrimaryForegroundColor.Light);
                UpdateColor(this._secondaryThemeFontColorBrush, palette.SecondaryForegroundColor.Light);
                
                UpdateColor(this._selectedLineFontColorBrush, palette.SelectedLineColor.Light);
                UpdateColor(this._unselectedLineFontColorBrush, palette.UnSelectedLineColor.Light);
            }
            else
            {
                UpdateColor(this._primaryThemeFontColorBrush, palette.PrimaryForegroundColor.Dark);
                UpdateColor(this._secondaryThemeFontColorBrush, palette.SecondaryForegroundColor.Dark);
                
                UpdateColor(this._selectedLineFontColorBrush, palette.SelectedLineColor.Dark);
                UpdateColor(this._unselectedLineFontColorBrush, palette.UnSelectedLineColor.Dark);
            }
            
            UpdateColor(this._lightThemeFontColorBrush, palette.PrimaryColor.Light);
            
        });

        artwork.ArtworkApplied = true;
    }

    private Color GetColor(Artwork artwork)
    {
        try
        {
            LabClusterColorCalculator labCalculator = new LabClusterColorCalculator();
            labCalculator.PreProcessing.BlurPreProcessing = true;
            return labCalculator.GetColorFromBitmap(artwork.ArtworkAsImage);
        }
        catch (Exception e)
        {
            // Change the default color to something pastel like, cuz that is the drip
            RGBToLabConverter converter = new RGBToLabConverter();

            Color defaultPastel = this._defaultColor
                .Normalize()
                .ToRgbColor()
                .ToLabColor(converter)
                .ToPastel()
                .ToRgbColor(converter)
                .DeNormalize();

            return defaultPastel;
        }
    }

    // Helper functions
    private SolidColorBrush FromResource(string resourceName)
    {
        return App.Current.FindResource(resourceName) as SolidColorBrush;
    }

    private void UpdateColor(SolidColorBrush colorBrush, Color color)
    {
        colorBrush.Color = color;
    }
}
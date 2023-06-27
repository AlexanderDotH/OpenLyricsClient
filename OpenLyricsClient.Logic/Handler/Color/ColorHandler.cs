using Avalonia.Media;
using Avalonia.Threading;
using DevBase.Avalonia.Color.Extensions;
using DevBase.Avalonia.Extension.Color.Image;
using DevBase.Avalonia.Extension.Converter;
using DevBase.Avalonia.Extension.Extension;
using OpenLyricsClient.Logic.Events.EventArgs;
using OpenLyricsClient.Logic.Events.EventHandler;
using OpenLyricsClient.Shared.Structure.Palette;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Logic.Handler.Color;

using Artwork = Shared.Structure.Artwork.Artwork;

public class ColorHandler
{
    private Avalonia.Media.Color _defaultColor;

    private readonly double _lightAdjustment = 80d;
    
    public event ColorPaletteChangedEventHandler ColorPaletteChangedEvent;
    
    public ColorHandler()
    {
        this._defaultColor = new Avalonia.Media.Color(255, 220, 20, 60);

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

        Avalonia.Media.Color color = GetColor(artwork);

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
        
        primaryForeground.Dark = new Avalonia.Media.Color(255, 255, 255, 255);
        primaryForeground.Light = new Avalonia.Media.Color(255, 22, 22, 22);

        secondaryForeground.Dark = new Avalonia.Media.Color(255,255,255,255).AdjustBrightness(90d);
        secondaryForeground.Light = new Avalonia.Media.Color(255,22, 22, 22).AdjustBrightness(110d);
        
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

        ColorPaletteChanged(artwork.ArtworkColor);

        artwork.ArtworkApplied = true;
    }

    private Avalonia.Media.Color GetColor(Artwork artwork)
    {
        try
        {
            LabClusterColorCalculator labCalculator = new LabClusterColorCalculator();
            
            /*labCalculator.Filter.BrightnessConfiguration.FilterBrightness = true;
            
            labCalculator.Filter.BrightnessConfiguration.MinBrightness = 10d;
            labCalculator.Filter.BrightnessConfiguration.MaxBrightness = 70d;

            labCalculator.Filter.ChromaConfiguration.MinChroma = 10d;*/
            
            labCalculator.UsePredefinedSet = true;
            
            labCalculator.PreProcessing.BlurPreProcessing = false;

            labCalculator.Clusters = 20;
            
            return labCalculator.GetColorFromBitmap(artwork.ArtworkAsImage);
        }
        catch (Exception e)
        {
            // Change the default color to something pastel like, cuz that is the drip
            RGBToLabConverter converter = new RGBToLabConverter();

            Avalonia.Media.Color defaultPastel = this._defaultColor
                .Normalize()
                .ToRgbColor()
                .ToLabColor(converter)
                .ToPastel()
                .ToRgbColor(converter)
                .DeNormalize();

            return defaultPastel;
        }
    }
    

    private void UpdateColor(SolidColorBrush colorBrush, Avalonia.Media.Color color)
    {
        colorBrush.Color = color;
    }
    
    protected virtual void ColorPaletteChanged(ColorPalette palette)
    {
        ColorPaletteChangedEventHandler colorPaletteChanged = ColorPaletteChangedEvent;
        colorPaletteChanged?.Invoke(this, new ColorPaletteChangedEventArgs(palette));
    }
}
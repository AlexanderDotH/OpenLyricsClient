using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using OpenLyricsClient.Shared.Utils;
using SkiaSharp;

namespace OpenLyricsClient.UI.Models.Elements.Blur;

/// <summary>
/// This is the blur render operation.
///
/// DISCLAIMER
/// This is not my code its from this github user: https://gist.github.com/kekekeks
///
/// Real source: https://gist.github.com/kekekeks/ac06098a74fe87d49a9ff9ea37fa67bc
/// 
/// </summary>
class BlurBehindRenderOperation : ICustomDrawOperation
{
    private readonly ImmutableExperimentalAcrylicMaterial _material;
    private Rect _bounds;
    private SKShader _shader;
    
    private float _sigmaX;
    private float _sigmaY;

    private float _cornerRadius;

    private bool _useNoise;
    private double _noiseOpacity;

    public BlurBehindRenderOperation(
        ImmutableExperimentalAcrylicMaterial material)
    {
        this._material = material;
    }
    
    public void Dispose()
    {

    }

    public bool HitTest(Point p) => _bounds.Contains(p);

    static SKColorFilter CreateAlphaColorFilter(double opacity)
    {
        if (opacity > 1)
            opacity = 1;
        
        byte[] c = new byte[256];
        byte[] a = new byte[256];
        
        for (int i = 0; i < 256; i++)
        {
            c[i] = (byte)i;
            a[i] = (byte)(i * opacity);
        }

        return SKColorFilter.CreateTable(a, c, c, c);
    }
    
    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        
        if (leaseFeature == null)
            return;
        
        using var skia = leaseFeature.Lease();
        
        if (!skia.SkCanvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
            return;

        using SKImage backgroundSnapshot = skia.SkSurface.Snapshot();
        
        using var backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp,
            SKShaderTileMode.Clamp, currentInvertedTransform);

        if (!DataValidator.ValidateData(skia.GrContext))
            return;
        
        using SKSurface blurred = SKSurface.Create(
            skia?.GrContext, 
            true, 
            new SKImageInfo(
                (int)Math.Ceiling(this._bounds.Width), 
                (int)Math.Ceiling(this._bounds.Height), 
                SKImageInfo.PlatformColorType, 
                SKAlphaType.Premul));
        
        if (blurred == null)
            return;

        SKRect rect = new SKRect(
            (float)_bounds.Left, 
            (float)_bounds.Top, 
            (float)_bounds.Right,
            (float)_bounds.Bottom);

        using (SKImageFilter filter = SKImageFilter.CreateBlur(this._sigmaX, this._sigmaY, SKShaderTileMode.Clamp))
            
        using (SKPaint blurPaint = new SKPaint
               {
                   Shader = backdropShader,
                   ImageFilter = filter,
                   FilterQuality = SKFilterQuality.Low
               })
            
        blurred.Canvas.DrawRect(rect, blurPaint);

        using (SKImage blurSnap = blurred.Snapshot())

        using (SKShader blurSnapShader = SKShader.CreateImage(blurSnap))

        using (SKPaint blurSnapPaint = new SKPaint
               {
                   Shader = blurSnapShader
               })
            
        skia.SkCanvas.DrawRoundRect(rect, this._cornerRadius, this._cornerRadius, blurSnapPaint);

        if (!UseNoise)
            return;
        
        using SKPaint acrylliPaint = new SKPaint();

        acrylliPaint.IsAntialias = true;

        double opacity = 0;

        Color tintColor = this._material.TintColor;
        SKColor tint = new SKColor(tintColor.R, tintColor.G, tintColor.B, tintColor.A);

        if (this._shader == null)
        {
            using (Stream stream =
                   typeof(SkiaPlatform).Assembly.GetManifestResourceStream(
                           "Avalonia.Skia.Assets.NoiseAsset_256X256_PNG.png"))
                
            using (SKBitmap bitmap = SKBitmap.Decode(stream))
            {
                this._shader = SKShader
                    .CreateBitmap(bitmap, SKShaderTileMode.Repeat, SKShaderTileMode.Repeat)
                    .WithColorFilter(CreateAlphaColorFilter(this._useNoise ? this._noiseOpacity : 0));
            }
        }

        using (var backdrop = SKShader.CreateColor(
                   new SKColor(
                       this._material.MaterialColor.R, 
                       this._material.MaterialColor.G,
                   this._material.MaterialColor.B, 
                       this._material.MaterialColor.A)))
            
        using (SKShader tintShader = SKShader.CreateColor(tint))
        using (SKShader effectiveTint = SKShader.CreateCompose(backdrop, tintShader))
        using (SKShader compose = SKShader.CreateCompose(effectiveTint, this._shader))
        {
            acrylliPaint.Shader = compose;
            acrylliPaint.IsAntialias = true;
            skia.SkCanvas.DrawRoundRect(rect, this._cornerRadius, this._cornerRadius, acrylliPaint);
        }
    }

    public float SigmaX
    {
        get => _sigmaX;
        set => _sigmaX = value;
    }

    public float SigmaY
    {
        get => _sigmaY;
        set => _sigmaY = value;
    }

    public float CornerRadius
    {
        get => _cornerRadius;
        set => _cornerRadius = value;
    }

    public bool UseNoise
    {
        get => _useNoise;
        set => _useNoise = value;
    }

    public double NoiseOpacity
    {
        get => _noiseOpacity;
        set => _noiseOpacity = value;
    }
    
    public Rect Bounds
    {
        get => this._bounds;
        set => this._bounds = value;
    }

    public bool Equals(ICustomDrawOperation? other)
    {
        return other is BlurBehindRenderOperation op && op._bounds == _bounds && op._material.Equals(_material);
    }
}
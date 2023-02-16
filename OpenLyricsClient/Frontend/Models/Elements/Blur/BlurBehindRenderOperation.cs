using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using CefNet;
using OpenLyricsClient.Backend.Utils;
using SkiaSharp;

namespace OpenLyricsClient.Frontend.Models.Elements.Blur;

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
    private readonly Rect _bounds;
    private SKShader _shader;
    
    private float _sigmaX;
    private float _sigmaY;

    private bool _useNoise;
    private double _noiseOpacity;

    public BlurBehindRenderOperation(
        ImmutableExperimentalAcrylicMaterial material,
        float sigmaX, 
        float sigmaY, 
        bool useNoise, 
        double noiseOpacity, Rect bounds)
    {
        this._material = material;
        this._bounds = bounds;
        
        this._sigmaX = sigmaX;
        this._sigmaY = sigmaY;

        this._useNoise = useNoise;
        this._noiseOpacity = noiseOpacity;
    }

    public Rect Bounds => _bounds.Inflate(4);
    
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

    public void Render(IDrawingContextImpl context)
    {
        if (context is not ISkiaDrawingContextImpl skia)
            return;

        if (!skia.SkCanvas.TotalMatrix.TryInvert(out var currentInvertedTransform))
            return;

        using SKImage backgroundSnapshot = skia.SkSurface.Snapshot();
        using var backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp,
            SKShaderTileMode.Clamp, currentInvertedTransform);

        if (!DataValidator.ValidateData(skia.GrContext))
            return;
        
        using SKSurface blurred = SKSurface.Create(skia?.GrContext, false, new SKImageInfo(
            (int)Math.Ceiling(this._bounds.Width),
            (int)Math.Ceiling(this._bounds.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul));
        
        using (SKImageFilter filter = SKImageFilter.CreateBlur(this._sigmaX, this._sigmaY, SKShaderTileMode.Clamp))
            
        using (SKPaint blurPaint = new SKPaint
               {
                   Shader = backdropShader,
                   ImageFilter = filter
               })

        blurred.Canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurPaint);

        using (SKImage blurSnap = blurred.Snapshot())

        using (SKShader blurSnapShader = SKShader.CreateImage(blurSnap))

        using (SKPaint blurSnapPaint = new SKPaint
               {
                   Shader = blurSnapShader,
                   IsAntialias = true
               })
            
        skia.SkCanvas.DrawRect(0, 0, (float)this._bounds.Width, (float)this._bounds.Height, blurSnapPaint);

            //return;
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
            skia.SkCanvas.DrawRect(0, 0, (float)this._bounds.Width, (float)this._bounds.Height, acrylliPaint);
        }
    }
    
    public bool Equals(ICustomDrawOperation? other)
    {
        return other is BlurBehindRenderOperation op && op._bounds == _bounds && op._material.Equals(_material);
    }
}
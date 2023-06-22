using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.UI.Models.Elements.Blur;

/// <summary>
/// This is the blur area.
///
/// DISCLAIMER
/// This is not my code its from this github user: https://gist.github.com/kekekeks
///
/// Real source: https://gist.github.com/kekekeks/ac06098a74fe87d49a9ff9ea37fa67bc
/// 
/// </summary>
public class BlurArea : Control
{
    public static StyledProperty<float> SigmaXProperty =
        AvaloniaProperty.Register<BlurArea, float>(nameof(SigmaX));
    
    public static StyledProperty<float> SigmaYProperty =
        AvaloniaProperty.Register<BlurArea, float>(nameof(SigmaY));
    
    public static StyledProperty<bool> UseNoiseProperty =
        AvaloniaProperty.Register<BlurArea, bool>(nameof(UseNoise));
    
    public static StyledProperty<double> NoiseOpacityProperty =
        AvaloniaProperty.Register<BlurArea, double>(nameof(NoiseOpacity));
    
    public static readonly StyledProperty<ExperimentalAcrylicMaterial> MaterialProperty = AvaloniaProperty.Register<BlurArea, ExperimentalAcrylicMaterial>(
            "Material");

    private ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterial;

    private BlurBehindRenderOperation _behindRenderOperation;

    private float _sigmaX;
    private float _sigmaY;
    
    private LyricPart _lyricPart;
    
    public BlurArea()
    {
        Sigma = 3;
        SigmaX = 3;
        SigmaY = 3;
        UseNoise = false;
        NoiseOpacity = 0.0225;
        
        ExperimentalAcrylicMaterial experimentalAcrylicMaterial = new ExperimentalAcrylicMaterial()
        {
            MaterialOpacity = 0.0,
            TintColor = Colors.Black,
            TintOpacity = 0.0,
            PlatformTransparencyCompensationLevel = 0
        };
        
        DefaultAcrylicMaterial = (ImmutableExperimentalAcrylicMaterial)experimentalAcrylicMaterial.ToImmutable();

        Material = experimentalAcrylicMaterial;
        
        //LyricsScroller.INSTANCE.BlurChanged += INSTANCEOnBlurChanged;
        
        AffectsRender<BlurArea>(MaterialProperty);
    }

    /*private void INSTANCEOnBlurChanged(object sender, BlurChangedEventArgs blurchangedevent)
    {
        if (this._lyricPart == null)
            return;
        
        if (blurchangedevent.LyricPart == this._lyricPart)
        {
            this.Sigma = blurchangedevent.BlurSigma;
            this.InvalidateVisual();
        }
    }*/

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        
        ImmutableExperimentalAcrylicMaterial material = Material != null
            ? (ImmutableExperimentalAcrylicMaterial)Material.ToImmutable()
            : DefaultAcrylicMaterial;
        
        this._behindRenderOperation = new BlurBehindRenderOperation(
            material, 
            this.SigmaX, 
            this.SigmaY, 
            this.UseNoise,
            this.NoiseOpacity,
            new Rect(default, Bounds.Size));
        
        if (!DataValidator.ValidateData(this._behindRenderOperation))
            return;
        
        context?.Custom(this._behindRenderOperation);
    }
    
    public bool UseNoise
    {
        get => GetValue(UseNoiseProperty);
        set => SetValue(UseNoiseProperty, value);
    }
    
    public double NoiseOpacity
    {
        get => GetValue(NoiseOpacityProperty);
        set => SetValue(NoiseOpacityProperty, value);
    }
    
    public LyricPart LyricPart
    {
        get => _lyricPart;
        set => _lyricPart = value;
    }

    public float SigmaX
    {
        get => _sigmaX;
        set
        {
            _sigmaX = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SigmaX"));
            InvalidateVisual();
        }
    }
    
    public float SigmaY
    {
        get => _sigmaY;
        set
        {
            _sigmaY = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SigmaX"));
            InvalidateVisual();
        }
    }

    public float Sigma
    {
        get
        {
            return (SigmaX + SigmaY) / 2; 
        }
        set
        {
            SigmaX = value;
            SigmaY = value;
            InvalidateVisual();
        }
    }
    
    public ExperimentalAcrylicMaterial Material
    {
        get => GetValue(MaterialProperty);
        set
        {
            SetValue(MaterialProperty, value);
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
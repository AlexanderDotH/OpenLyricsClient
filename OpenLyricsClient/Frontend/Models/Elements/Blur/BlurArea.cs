using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace OpenLyricsClient.Frontend.Models.Elements.Blur;

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

    public BlurArea()
    {
        ExperimentalAcrylicMaterial experimentalAcrylicMaterial = new ExperimentalAcrylicMaterial()
        {
            MaterialOpacity = 0.0,
            TintColor = Colors.Black,
            TintOpacity = 0.0,
            PlatformTransparencyCompensationLevel = 0
        };
        
        DefaultAcrylicMaterial = (ImmutableExperimentalAcrylicMaterial)experimentalAcrylicMaterial.ToImmutable();

        Material = experimentalAcrylicMaterial;

        Sigma = 3;
        UseNoise = false;
        NoiseOpacity = 0.0225;
        
        AffectsRender<BlurArea>(MaterialProperty);
    }
    
    public override void Render(DrawingContext context)
    {
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
        
        context.Custom(this._behindRenderOperation);
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
    
    public float SigmaX
    {
        get => GetValue(SigmaXProperty);
        set => SetValue(SigmaXProperty, value);
    }

    public float SigmaY
    {
        get => GetValue(SigmaYProperty);
        set => SetValue(SigmaYProperty, value);
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
}
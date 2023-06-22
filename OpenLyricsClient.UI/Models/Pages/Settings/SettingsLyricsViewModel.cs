using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Settings.Sections.Lyrics;
using OpenLyricsClient.Shared.Structure.Enum;
using ReactiveUI;

namespace OpenLyricsClient.UI.Models.Pages.Settings;

public class SettingsLyricsViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ReactiveCommand<Unit, Unit> SwitchToKaraokeModeCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToFadeModeCommand { get; }
    
    public ReactiveCommand<Unit, Unit> ToggleArtworkBackgroundCommand { get; }
    public ReactiveCommand<Unit, Unit> ToggleLyricsBlurCommand { get; }

    public SettingsLyricsViewModel()
    {
        SwitchToKaraokeModeCommand = ReactiveCommand.Create(SwitchToKaraoke);
        SwitchToFadeModeCommand = ReactiveCommand.Create(SwitchToFade);
        ToggleArtworkBackgroundCommand = ReactiveCommand.Create(ToggleArtworkBackground);
        ToggleLyricsBlurCommand = ReactiveCommand.Create(ToggleLyricsBlur);
    }

    private void SwitchToKaraoke()
    {
        Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?
            .SetValue("Selection Mode", EnumLyricsDisplayMode.KARAOKE);
        Core.INSTANCE.SettingsHandler.TriggerEvent(this, "Selection Mode");
    }
    
    private void SwitchToFade()
    {
        Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?
            .SetValue("Selection Mode", EnumLyricsDisplayMode.FADE);
        Core.INSTANCE.SettingsHandler.TriggerEvent(this, "Selection Mode");
    }
    
    private void ToggleArtworkBackground()
    {
        Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?
            .SetValue("Artwork Background", !UseArtworkBackground);
        Core.INSTANCE.SettingsHandler.TriggerEvent(this, "Artwork Background");
    }
    
    private void ToggleLyricsBlur()
    {
        Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?
            .SetValue("Blur Lyrics", !IsBlurred);
        Core.INSTANCE.SettingsHandler.TriggerEvent(this, "Blur Lyrics");
    }

    public bool IsKaraoke
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?.GetValue<EnumLyricsDisplayMode>("Selection Mode") == EnumLyricsDisplayMode.KARAOKE;
    }
    
    public bool IsFade
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?.GetValue<EnumLyricsDisplayMode>("Selection Mode") == EnumLyricsDisplayMode.FADE;
    }
    
    public bool UseArtworkBackground
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?.GetValue<bool>("Artwork Background") == true;
    }
    
    public bool IsBlurred
    {
        get => Core.INSTANCE.SettingsHandler.Settings<LyricsSection>()?.GetValue<bool>("Blur Lyrics") == true;
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
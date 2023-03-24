using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Romanization;
using OpenLyricsClient.Backend.Structure.Enum;
using ReactiveUI;

namespace OpenLyricsClient.Frontend.Models.Pages.Settings;

public class SettingsLyricsViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ReactiveCommand<Unit, Unit> SwitchToKaraokeModeCommand { get; }
    public ReactiveCommand<Unit, Unit> SwitchToFadeModeCommand { get; }
    
    public SettingsLyricsViewModel()
    {
        SwitchToKaraokeModeCommand = ReactiveCommand.Create(SwitchToKaraoke);
        SwitchToFadeModeCommand = ReactiveCommand.Create(SwitchToFade);
    }

    private void SwitchToKaraoke()
    {
        Core.INSTANCE.SettingManager.Settings.DisplayPreferences.DisplayMode = EnumLyricsDisplayMode.KARAOKE;
        Core.INSTANCE.SettingManager.WriteSettings();
    }
    
    private void SwitchToFade()
    {
        Core.INSTANCE.SettingManager.Settings.DisplayPreferences.DisplayMode = EnumLyricsDisplayMode.FADE;
        Core.INSTANCE.SettingManager.WriteSettings();
    }

    public bool IsKaraoke
    {
        get => Core.INSTANCE.SettingManager.Settings.DisplayPreferences.DisplayMode == EnumLyricsDisplayMode.KARAOKE;
    }
    
    public bool IsFade
    {
        get => Core.INSTANCE.SettingManager.Settings.DisplayPreferences.DisplayMode == EnumLyricsDisplayMode.FADE;
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
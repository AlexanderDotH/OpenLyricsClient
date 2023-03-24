using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Runtime.CompilerServices;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Romanization;
using ReactiveUI;

namespace OpenLyricsClient.Frontend.Models.Pages.Settings;

public class SettingsRomanizationViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ReactiveCommand<Unit, Unit> JapaneseCommand { get; }
    public ReactiveCommand<Unit, Unit> KoreanCommand { get; }
    public ReactiveCommand<Unit, Unit> RussianCommand { get; }
    
    public SettingsRomanizationViewModel()
    {
        JapaneseCommand = ReactiveCommand.Create(Japanese);
        KoreanCommand = ReactiveCommand.Create(Korean);
        RussianCommand = ReactiveCommand.Create(Russian);
    }
    
    private void CheckOrUncheckAndWrite(RomanizeSelection selection)
    {
        if (Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(selection))
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Remove(selection);
        }
        else
        {
            Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Add(selection);
        }
        
        Core.INSTANCE.SettingManager.WriteSettings();
    }

    private bool IsAvailable(RomanizeSelection selection) =>
        Core.INSTANCE.SettingManager.Settings.RomanizeSelection.Contains(selection);
    
    private void Japanese()
    {
        CheckOrUncheckAndWrite(RomanizeSelection.JAPANESE_TO_ROMANJI);
    }
    
    private void Korean()
    {
        CheckOrUncheckAndWrite(RomanizeSelection.KOREAN_TO_ROMANJI);
    }
    
    private void Russian()
    {
        CheckOrUncheckAndWrite(RomanizeSelection.RUSSIA_TO_LATIN);
    }

    public bool IsJapaneseEnabled
    {
        get => IsAvailable(RomanizeSelection.JAPANESE_TO_ROMANJI);
    }
    
    public bool IsKoreanEnabled
    {
        get => IsAvailable(RomanizeSelection.KOREAN_TO_ROMANJI);
    }
    
    public bool IsRussianEnabled
    {
        get => IsAvailable(RomanizeSelection.RUSSIA_TO_LATIN);
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
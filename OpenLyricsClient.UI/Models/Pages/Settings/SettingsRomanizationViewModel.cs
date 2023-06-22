using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OpenLyricsClient.Logic;
using OpenLyricsClient.Logic.Settings.Sections.Romanization;
using OpenLyricsClient.Shared.Structure.Romanization;
using ReactiveUI;

namespace OpenLyricsClient.UI.Models.Pages.Settings;

public class SettingsRomanizationViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ReactiveCommand<Unit, Unit> JapaneseCommand { get; }
    public ReactiveCommand<Unit, Unit> KoreanCommand { get; }
    public ReactiveCommand<Unit, Unit> RussianCommand { get; }
    
    public SettingsRomanizationViewModel()
    {
        JapaneseCommand = ReactiveCommand.CreateFromTask(Japanese);
        KoreanCommand = ReactiveCommand.CreateFromTask(Korean);
        RussianCommand = ReactiveCommand.CreateFromTask(Russian);
    }
    
    private async Task CheckOrUncheckAndWrite(RomanizeSelection selection)
    {
        if (Core.INSTANCE.SettingsHandler.Settings<RomanizationSection>()!.ContainsdRomanization(selection))
        {
            await Core.INSTANCE.SettingsHandler.Settings<RomanizationSection>()?.RemoveRomanization(selection);
        }
        else
        {
            await Core.INSTANCE.SettingsHandler.Settings<RomanizationSection>()?.AddRomanization(selection);
        }

        await Core.INSTANCE.SettingsHandler?.TriggerEvent(typeof(RomanizationSection), "Selections");
    }

    private bool IsAvailable(RomanizeSelection selection) =>
        Core.INSTANCE.SettingsHandler.Settings<RomanizationSection>()!.ContainsdRomanization(selection);
    
    private async Task Japanese()
    {
        await CheckOrUncheckAndWrite(RomanizeSelection.JAPANESE_TO_ROMANJI);
    }
    
    private async Task Korean()
    {
        await CheckOrUncheckAndWrite(RomanizeSelection.KOREAN_TO_ROMANJI);
    }
    
    private async Task Russian()
    {
        await CheckOrUncheckAndWrite(RomanizeSelection.RUSSIA_TO_LATIN);
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
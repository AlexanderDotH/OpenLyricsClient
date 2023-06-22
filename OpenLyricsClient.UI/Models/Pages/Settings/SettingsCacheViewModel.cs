using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using OpenLyricsClient.Logic;
using ReactiveUI;

namespace OpenLyricsClient.UI.Models.Pages.Settings;

public class SettingsCacheViewModel : ViewModelBase, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public ReactiveCommand<Unit, Unit> ClearCacheCommand { get; }
    
    public SettingsCacheViewModel()
    {
        ClearCacheCommand = ReactiveCommand.Create(ClearCache);
    }
    
    public void ClearCache()
    {
        Core.INSTANCE.CacheManager.ClearCache();
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
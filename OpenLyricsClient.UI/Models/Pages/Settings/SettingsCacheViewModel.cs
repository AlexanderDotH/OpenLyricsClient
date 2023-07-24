using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive;
using System.Runtime.CompilerServices;
using OpenLyricsClient.Logic;
using ReactiveUI;

namespace OpenLyricsClient.UI.Models.Pages.Settings;

public class SettingsCacheViewModel : ModelBase
{
    public ReactiveCommand<Unit, Unit> ClearCacheCommand { get; }
    
    public SettingsCacheViewModel()
    {
        ClearCacheCommand = ReactiveCommand.Create(ClearCache);
    }
    
    public void ClearCache()
    {
        Core.INSTANCE.CacheManager.ClearCache();
    }
}
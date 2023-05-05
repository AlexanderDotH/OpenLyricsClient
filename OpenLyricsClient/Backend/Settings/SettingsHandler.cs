using System.IO;
using System.Threading.Tasks;
using Avalonia.Threading;
using DevBase.Generics;
using OpenLyricsClient.Backend.Events.EventArgs;
using OpenLyricsClient.Backend.Events.EventHandler;
using OpenLyricsClient.Backend.Settings.Sections;
using OpenLyricsClient.Backend.Settings.Sections.Connection.Spotify;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Backend.Settings.Sections.Romanization;
using OpenLyricsClient.Backend.Settings.Sections.Tokens;
using OpenLyricsClient.Shared.Structure.Enum;

namespace OpenLyricsClient.Backend.Settings;

public class SettingsHandler
{
    private AList<ISettingSection> _sections;

    public event SettingsChangedEventHandler SettingsChanged;
    
    public SettingsHandler(string workingDirectory)
    {
        SetupWorkingDirectory(workingDirectory);
        
        this._sections = new AList<ISettingSection>();

        this._sections.Add(new LyricsSection(string.Format("{0}{1}", 
            workingDirectory, "Lyrics Preferences.json")));
        
        this._sections.Add(new SpotifySection(string.Format("{0}{1}", 
            workingDirectory, "Spotify Access.json")));
        
        this._sections.Add(new TokenSection(string.Format("{0}{1}", 
            workingDirectory, "Tokens.json")));
        
        this._sections.Add(new RomanizationSection(string.Format("{0}{1}", 
            workingDirectory, "Romanization.json")));
        
        this._sections.Add(new AccountSection(string.Format("{0}{1}", 
            workingDirectory, "Account.json")));

        this._sections.Add(new PluginsSection(string.Format("{0}{1}",
            workingDirectory, "Plugins.json"))); // Path.Join()

        Task.Factory.StartNew(Initialize).GetAwaiter().GetResult();
    }

    private void SetupWorkingDirectory(string workingDirectory)
    {
        if (!Directory.Exists(workingDirectory))
            Directory.CreateDirectory(workingDirectory);
    }
    
    public async Task Initialize()
    {
        this._sections.ForEach(async t=> await t.ReadFromDisk());
    }

    public T? Settings<T>() where T : ISettingSection
    {
        return (T)this._sections.GetAsList().Find(s => s is T);
    }

    public async Task TriggerEvent<T>(T section, string field)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            SettingsChangedEventArgs args = new SettingsChangedEventArgs
            {
                Section = section,
                Field = field
            };

            SettingsChangedEventHandler settingsChangedEventHandler = SettingsChanged;
            settingsChangedEventHandler?.Invoke(this, args);
        });
    }

    public async Task TriggerGlobal()
    {
        for (int i = 0; i < this._sections.Length; i++)
        {
            ISettingSection section = this._sections.Get(i);
            
            string[] fields = section.GetFields();
            
            for (int j = 0; j < fields.Length; j++)
                await TriggerEvent(section, fields[j]);
        }
    }
}
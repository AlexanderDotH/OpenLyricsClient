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
using OpenLyricsClient.Backend.Structure.Enum;

namespace OpenLyricsClient.Backend.Settings;

public class SettingsHandler
{
    private AList<ISettingSection> _sections;

    public event SettingsChangedEventHandler SettingsChanged;
    
    public SettingsHandler(string workingDirectory)
    {
        this._sections = new AList<ISettingSection>();

        this._sections.Add(new LyricsSection(string.Format("{0}{1}", 
            workingDirectory, "Lyrics Preferences.json")));
        
        this._sections.Add(new SpotifySection(string.Format("{0}{1}", 
            workingDirectory, "Spotify Access.json")));
        
        this._sections.Add(new TokenSection(string.Format("{0}{1}", 
            workingDirectory, "Tokens.json")));
        
        this._sections.Add(new RomanizationSection(string.Format("{0}{1}", 
            workingDirectory, "Romanization.json")));

        Task.Factory.StartNew(Initialize).GetAwaiter().GetResult();
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
        this._sections.ForEach(s =>
        {
            using (var enumerator = s.Defaults().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    TriggerEvent(s, enumerator.Current.Key.ToString());
                }
            }
        });
    }
}
using System.Threading.Tasks;
using DevBase.Generics;
using OpenLyricsClient.Backend.Settings.Sections;
using OpenLyricsClient.Backend.Settings.Sections.Lyrics;
using OpenLyricsClient.Backend.Structure.Enum;

namespace OpenLyricsClient.Backend.Settings;

public class SettingsHandler
{
    private AList<ISettingSection> _sections;

    public SettingsHandler(string workingDirectory)
    {
        this._sections = new AList<ISettingSection>();

        this._sections.Add(new LyricsSection(string.Format("{0}{1}", 
            workingDirectory, "lyrics-preferences.json")));

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
}
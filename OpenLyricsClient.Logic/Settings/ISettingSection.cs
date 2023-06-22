using Newtonsoft.Json.Linq;

namespace OpenLyricsClient.Logic.Settings;

public interface ISettingSection
{
    Task WriteToDisk();
    Task ReadFromDisk();
    T GetValue<T>(string field);
    Task SetValue<T>(string field, T value);
    JObject Defaults();
    string[] GetFields();
}
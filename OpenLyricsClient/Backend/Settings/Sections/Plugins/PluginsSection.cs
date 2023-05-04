using System.IO;
using System.Threading.Tasks;
using DevBase.Generics;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Backend.Settings;
using OpenLyricsClient.Backend.Settings.Sections.Plugins;
using OpenLyricsClient.Shared.Utils;

public class PluginsSection : ISettingSection
{
    private FileInfo _file;
    private JObject _data;

    public PluginsSection(string filePath)
    {
        this._file = new FileInfo(filePath);
    }

    public async Task WriteToDisk()
    {
        string encoded = Core.INSTANCE.Sealing.SimpleEncrypt(this._data.ToString());
        await File.WriteAllTextAsync(this._file.FullName, encoded);
    }

    public async Task ReadFromDisk()
    {
        if (!this._file.Exists)
        {
            this._data = Defaults();
            await WriteToDisk();
            return;
        }

        await using FileStream stream = this._file.OpenRead();
        using StreamReader reader = new StreamReader(stream);

        string decoded = Core.INSTANCE.Sealing.SimpleDecrypt(reader.ReadToEnd());

        this._data = JObject.Parse(decoded);

        await stream.FlushAsync();

        stream.Close();
        reader.Close();
    }

    public T GetValue<T>(string field)
    {
        return (T)this._data[field].ToObject<T>();
    }

    public async Task SetValue<T>(string field, T value)
    {
        this._data[field] = JToken.FromObject(value);
        await WriteToDisk();
    }

    public JObject Defaults()
    {
        Structure structure = new Structure(); // TODO
        return new JsonDeserializer().Serialize(structure);
    }

    public string[] GetFields()
    {
        AList<string> fields = new AList<string>();

        foreach (var pair in this._data)
            fields.Add(pair.Key);

        return fields.GetAsArray();
    }
}
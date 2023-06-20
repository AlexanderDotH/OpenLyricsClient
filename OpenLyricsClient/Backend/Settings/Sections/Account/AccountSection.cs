using System.IO;
using System.Threading.Tasks;
using DevBase.Api.Apis.OpenLyricsClient.Structure.Json;
using DevBase.Generics;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Backend.Settings.Sections.Account;

public class AccountSection : ISettingSection
{
    private FileInfo _file;
    private JObject _data;

    public AccountSection(string filePath)
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
        JsonOpenLyricsClientSubscription account = 
            new DevBase.Api.Apis.OpenLyricsClient.OpenLyricsClient(Core.INSTANCE.Sealing.ServerPublicKey)
                .CreateSubscription()
                .GetAwaiter()
                .GetResult();
        
        Structure structure = new Structure
        {
            UserID = account.UserID,
            UserSecret = account.UserSecret
        };
        
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
using DevBase.Generics;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Logic.Romanization;
using OpenLyricsClient.Shared.Structure.Romanization;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Settings.Sections.Romanization;

public class RomanizationSection : ISettingSection
{
    private FileInfo _file;
    private JObject _data;

    public RomanizationSection(string filePath)
    {
        this._file = new FileInfo(filePath);
    }
    
    public async Task WriteToDisk()
    {
        await File.WriteAllTextAsync(this._file.FullName, this._data.ToString());
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

        this._data = JObject.Parse(reader.ReadToEnd());
        
        await stream.FlushAsync();
        
        stream.Close();
        reader.Close();
    }

    public async Task AddRomanization(RomanizeSelection selection)
    {
        List<RomanizeSelection> tokens = GetValue<List<RomanizeSelection>>("Selections");
        tokens.Add(selection);
        
        await SetValue("Selections", tokens);
    }
    
    public async Task RemoveRomanization(RomanizeSelection selection)
    {
        List<RomanizeSelection> tokens = GetValue<List<RomanizeSelection>>("Selections");
        tokens.Remove(selection);
        
        await SetValue("Selections", tokens);
    }

    public bool ContainsdRomanization(RomanizeSelection selection)
    {
        List<RomanizeSelection> tokens = GetValue<List<RomanizeSelection>>("Selections");
        return tokens.Contains(selection);
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
        Structure structure = new Structure
        {
            Selections = new List<RomanizeSelection>(
                new RomanizeSelection[]
                {
                    RomanizeSelection.RUSSIA_TO_LATIN, 
                    RomanizeSelection.KOREAN_TO_ROMANJI, 
                    RomanizeSelection.JAPANESE_TO_ROMANJI
                })
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
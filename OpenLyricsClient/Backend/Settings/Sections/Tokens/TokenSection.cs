using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using DevBase.Api.Serializer;
using DevBase.Generics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Backend.Structure;
using OpenLyricsClient.Backend.Structure.Enum;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Settings.Sections.Tokens;

public class TokenSection : ISettingSection
{
    private FileInfo _file;
    private JObject _data;

    public TokenSection(string filePath)
    {
        this._file = new FileInfo(filePath);
    }
    
    public async Task WriteToDisk()
    {
        try
        {
            await File.WriteAllTextAsync(this._file.FullName, this._data.ToString());
        }
        catch (Exception e) { }
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

    public async Task AddToken(MusixMatchToken token)
    {
        List<MusixMatchToken> tokens = GetValue<List<MusixMatchToken>>("Tokens");
        tokens.Add(token);
        
        await SetValue("Tokens", tokens);
    }
    
    public async Task RemoveToken(MusixMatchToken token)
    {
        List<MusixMatchToken> tokens = GetValue<List<MusixMatchToken>>("Tokens");
        tokens.Remove(token);
        
        await SetValue("Tokens", tokens);
    }
    
    public async Task RemoveUsage(MusixMatchToken token)
    {
        List<MusixMatchToken> tokens = GetValue<List<MusixMatchToken>>("Tokens");

        for (int i = 0; i < tokens.Capacity; i++)
        {
            MusixMatchToken currentToken = tokens[i];
            if (currentToken.Token.SequenceEqual(token.Token))
            {
                currentToken.Usage--;

                if (currentToken.Usage <= 0)
                    tokens.Remove(currentToken);
                
                await SetValue("Tokens", tokens);
            }
        }
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
            Tokens = new List<MusixMatchToken>()
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
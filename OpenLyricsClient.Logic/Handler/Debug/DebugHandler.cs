using System.Net;
using System.Reflection;
using Accord.MachineLearning.DecisionTrees.Pruning;
using DevBase.Generics;
using DevBase.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenLyricsClient.Logic.Debugger;
using OpenLyricsClient.Logic.Debugger.Structure;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Handler.Debug;

public class DebugHandler
{
    private AList<DebugEntry> _entries;
    private AList<DebugEntry> _globalEntries;

    private FileInfo _workingFile;
    private DirectoryInfo _workingDirectory;

    public DebugHandler(string workingDirectory)
    {
        this._entries = new AList<DebugEntry>();
        this._globalEntries = new AList<DebugEntry>();
        
        this._workingDirectory = new DirectoryInfo(workingDirectory);
        SetupWorkingDirectory();

        this._workingFile = new FileInfo(Path.Combine(this._workingDirectory.FullName,
            string.Format("Debug-log-{0}.log", DateTimeOffset.Now.ToUnixTimeMilliseconds())));
        
        ReadFromFile();
    }

    private void SetupWorkingDirectory()
    {
        if (!this._workingDirectory.Exists)
            this._workingDirectory.Create();
    }

    public void Write<T>(string message, DebugType debugType, T type)
    {
        DebugEntry entry = new DebugEntry()
        {
            Message = message,
            Timestamp = DateTimeOffset.Now,
            Type = debugType,
            Component = type.GetType().Name
        };
        
        this._entries.Add(entry);
        this._globalEntries.Add(entry);
        
        WriteToDisk();
    }

    public void Write<T>(Exception exception, T type) => Write<T>(string.Format("{0}\n{1}", exception.Message, exception.StackTrace!), DebugType.ERROR, type);

    private void ReadFromFile()
    {
        AList<AFileObject> files = AFile.GetFiles(this._workingDirectory.FullName, true, "*.log");
        List<DebugEntry> entries = new List<DebugEntry>();

        JsonDeserializer deserializer = new JsonDeserializer();
        
        files.ForEach(e =>
        {
            DebugFile file = deserializer.Deserialize<DebugFile>(e.ToStringData());
            
            if (DataValidator.ValidateData(file))
                entries.AddRange(file.Entries);
        });

        entries = entries.OrderByDescending(e => e.Timestamp.ToUnixTimeMilliseconds()).ToList();

        if (entries.Count > 100)
            entries = entries.GetRange(0, 100);
        
        this._globalEntries.AddRange(entries);
    }

    private void WriteToDisk()
    {
        DebugFile file = new DebugFile();

        if (this._workingFile.Exists)
        {
            this._workingFile.Create();
            file = JsonConvert.DeserializeObject<DebugFile>(FileUtils.ReadFileString(this._workingFile));
        }
        else
        {
            file.Entries = new List<DebugEntry>();
        }

        file.Entries = this._entries.GetAsList();
        
        FileUtils.WriteFileString(this._workingFile, JsonConvert.SerializeObject(file, Formatting.Indented));
    }
}
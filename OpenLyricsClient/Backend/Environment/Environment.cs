using System.IO;
using System.Reflection;
using DevBase.Format;
using DevBase.Format.Formats.EnvFormat;
using DevBase.Generic;
using DevBase.IO;

namespace OpenLyricsClient.Backend.Environment
{
    class Environment
    {
        private GenericTupleList<string, string> _elements;

        public Environment(string path)
        {
            FileFormatParser<GenericTupleList<string, string>> environmentParser =
                new FileFormatParser<GenericTupleList<string, string>>(new EnvParser<GenericTupleList<string, string>>());
            
            if (!File.Exists(path))
                return;
            
            this._elements = environmentParser.FormatFromFile(path);
        }
        
        public string Get(EnvironmentType type)
        {
            return Find(type.ToString());
        }

        private string Find(string key)
        {
            for (int i = 0; i < this._elements.Length; i++)
            {
                if (this._elements.Get(i).Item1.Equals(key))
                {
                    return this._elements.Get(i).Item2;
                }
            }

            return string.Empty;
        }

        public static Environment FindEnvironmentFile(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                if (arg.Contains("--envFile"))
                {
                    string[] split = arg.Split('=');
                    if (split.Length > 1)
                        return new Environment(split[1].Replace("\"", string.Empty));
                }
            }

            GenericList<AFileObject> files = AFile.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), false, "*.env");

            for (int i = 0; i < files.Length; i++)
            {
                AFileObject fileObject = files.Get(i);
                return new Environment(fileObject.FileInfo.FullName);
            }

            return null;
        }
    }
}

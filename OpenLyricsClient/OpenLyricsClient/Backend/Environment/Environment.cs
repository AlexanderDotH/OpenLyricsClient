using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using DevBaseFormat;
using DevBaseFormat.Formats.EnvFormat;
using DevBaseFormat.Structure;

namespace OpenLyricsClient.Backend.Environment
{
    class Environment
    {
        private GenericTupleList<string, string> _elements;

        public Environment(string path)
        {
            FileFormatParser<GenericTupleList<string, string>> environmentParser =
                new FileFormatParser<GenericTupleList<string, string>>(new EnvParser<GenericTupleList<string, string>>());
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

            return null;
        }
    }
}

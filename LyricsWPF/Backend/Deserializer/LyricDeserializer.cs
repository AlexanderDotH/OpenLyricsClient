using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Deserializer.JsonStructures;
using LyricsWPF.Backend.Exceptions;
using Newtonsoft.Json;

namespace LyricsWPF.Backend.Deserializer
{
    class LyricDeserializer
    {
        private string _fullLyrics;

        public LyricDeserializer(string fullLyrics)
        {
            this._fullLyrics = fullLyrics;
        }

        public JsonLyricPart[] deserialize()
        {
            if (this._fullLyrics == null)
                throw new LyricNotDeserializableException();

            if (this._fullLyrics.Equals("No lyrics available"))
                throw new LyricNotDeserializableException();

            return JsonConvert.DeserializeObject<JsonLyricPart[]>(this._fullLyrics);
        }

        public string FullLyrics
        {
            get { return this._fullLyrics; }
        }
    }
}

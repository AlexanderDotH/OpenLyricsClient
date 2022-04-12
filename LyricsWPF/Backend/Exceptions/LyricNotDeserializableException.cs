using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Exceptions
{
    class LyricNotDeserializableException : Exception
    {
        public LyricNotDeserializableException() : base("Lyrics could not be deserialized") { }
    }
}

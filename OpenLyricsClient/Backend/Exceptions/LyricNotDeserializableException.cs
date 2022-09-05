using System;

namespace OpenLyricsClient.Backend.Exceptions
{
    class LyricNotDeserializableException : Exception
    {
        public LyricNotDeserializableException() : base("Lyrics could not be deserialized") { }
    }
}

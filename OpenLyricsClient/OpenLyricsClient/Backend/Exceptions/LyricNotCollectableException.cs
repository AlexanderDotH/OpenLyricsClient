using System;

namespace OpenLyricsClient.Backend.Exceptions
{
    class LyricNotCollectableException : Exception
    {
        public LyricNotCollectableException() : base("Lyrics could not be collected"){}
    }
}

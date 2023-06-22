namespace OpenLyricsClient.Logic.Exceptions
{
    class LyricNotDeserializableException : Exception
    {
        public LyricNotDeserializableException() : base("Lyrics could not be deserialized") { }
    }
}

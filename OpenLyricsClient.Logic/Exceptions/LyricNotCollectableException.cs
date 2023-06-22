namespace OpenLyricsClient.Logic.Exceptions
{
    class LyricNotCollectableException : Exception
    {
        public LyricNotCollectableException() : base("Lyrics could not be collected"){}
    }
}

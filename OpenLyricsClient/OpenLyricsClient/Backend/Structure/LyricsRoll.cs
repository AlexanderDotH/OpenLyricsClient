namespace OpenLyricsClient.Backend.Structure
{
    public class LyricsRoll
    {
        private LyricPart _partOne;
        private LyricPart _partTwo;
        private LyricPart _partThree;
        private LyricPart _partFour;
        private LyricPart _partFive;

        public LyricsRoll(LyricPart partOne, LyricPart partTwo, LyricPart partThree, LyricPart partFour, LyricPart partFive)
        {
            _partOne = partOne;
            _partTwo = partTwo;
            _partThree = partThree;
            _partFour = partFour;
            _partFive = partFive;
        }

        public LyricPart PartOne
        {
            get => _partOne;
            set => _partOne = value;
        }

        public LyricPart PartTwo
        {
            get => _partTwo;
            set => _partTwo = value;
        }

        public LyricPart PartThree
        {
            get => _partThree;
            set => _partThree = value;
        }

        public LyricPart PartFour
        {
            get => _partFour;
            set => _partFour = value;
        }

        public LyricPart PartFive
        {
            get => _partFive;
            set => _partFive = value;
        }
    }
}

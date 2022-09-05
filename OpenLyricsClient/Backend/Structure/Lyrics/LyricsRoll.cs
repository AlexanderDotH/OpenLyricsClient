using System;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Structure.Lyrics
{
    [Serializable]
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

            Fix();
        }

        private void Fix()
        {
            if (!DataValidator.ValidateData(this._partOne))
                this._partOne = new LyricPart(0, "");

            if (!DataValidator.ValidateData(this._partTwo))
                this._partTwo = new LyricPart(0, "");

            if (!DataValidator.ValidateData(this._partThree))
                this._partThree = new LyricPart(0, "");

            if (!DataValidator.ValidateData(this._partFour))
                this._partFour = new LyricPart(0, "");

            if (!DataValidator.ValidateData(this._partFive))
                this._partFive = new LyricPart(0, "");
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

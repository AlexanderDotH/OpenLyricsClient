using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Structure
{
    class LyricsRoll
    {
        private LyricPart _partOne;
        private LyricPart _partTwo;
        private LyricPart _partThree;

        public LyricsRoll(LyricPart partOne, LyricPart partTwo, LyricPart partThree)
        {
            _partOne = partOne;
            _partTwo = partTwo;
            _partThree = partThree;
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
    }
}

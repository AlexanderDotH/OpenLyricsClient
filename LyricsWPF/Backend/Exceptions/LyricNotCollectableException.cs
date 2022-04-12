using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Exceptions
{
    class LyricNotCollectableException : Exception
    {
        public LyricNotCollectableException() : base("Lyrics could not be collected"){}
    }
}

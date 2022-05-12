using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Collector
{
    class CollectorComparer : IComparer<ICollector>
    {
        public int Compare(ICollector x, ICollector y)
        {
            if (x == null || y == null)
                return 0;

            return y.ProviderQuality().CompareTo(x.ProviderQuality());
        }
    }
}

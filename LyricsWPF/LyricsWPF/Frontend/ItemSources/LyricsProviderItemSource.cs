using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Frontend.ItemSources
{
    public class LyricsProviderItemSource
    {
        public IList<string> ListOfItems { get; set; }

        public LyricsProviderItemSource()
        {
            ListOfItems = new List<string>();
            ListOfItems.Add("Quality");
            ListOfItems.Add("Performance");
        }
    }
}

using System.Collections.Generic;

namespace OpenLyricsClient.Frontend.ItemSources
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

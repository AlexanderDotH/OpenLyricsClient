using OpenLyricsClient.Shared.Plugin;
using OpenLyricsClient.Shared.Structure.Lyrics;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Logic.Collector.Lyrics.Providers.Plugin
{
    internal class PluginLyricsCollector : ILyricsCollector
    {
        public string CollectorName()
        {
            return "Plugin";
        }

        public async Task<LyricData> GetLyrics(SongResponseObject songResponseObject)
        {
            LyricData collectedData = new LyricData();
            foreach (IPlugin plugin in Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.LyricsCollector).OrderByDescending((IPlugin plugin) => plugin.GetCollectedLyricsQuality()))
            {
                LyricData? data = await plugin.CollectLyrics(songResponseObject);
                if (data != null && data != collectedData)
                {
                    collectedData = data;
                    break;
                }
            }
            return collectedData;
        }

        public int ProviderQuality()
        {
            IPlugin? plugin = Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.LyricsCollector).MaxBy((IPlugin plugin) => plugin.GetCollectedLyricsQuality());
            if (plugin == null)
                return -1;
            return plugin.GetCollectedLyricsQuality();
        }
    }
}

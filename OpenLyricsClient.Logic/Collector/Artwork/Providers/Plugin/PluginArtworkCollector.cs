using OpenLyricsClient.Shared.Plugin;
using OpenLyricsClient.Shared.Structure.Song;

namespace OpenLyricsClient.Logic.Collector.Artwork.Providers.Plugin
{
    internal class PluginArtworkCollector : IArtworkCollector
    {
        public string CollectorName()
        {
            return "Plugin";
        }

        async public Task<Shared.Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject)
        {
            Shared.Structure.Artwork.Artwork collectedData = new Shared.Structure.Artwork.Artwork();
            foreach (IPlugin plugin in Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.ArtworkCollector).OrderByDescending((IPlugin plugin) => plugin.GetCollectedArtworkQuality()))
            {
                Shared.Structure.Artwork.Artwork? data = await plugin.CollectArtwork(songResponseObject);
                if (data != null && data != collectedData)
                {
                    collectedData = data;
                    break;
                }
            }
            return collectedData;
        }

        public int Quality()
        {
            IPlugin? plugin = Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.ArtworkCollector).MaxBy((IPlugin plugin) => plugin.GetCollectedArtworkQuality());
            if (plugin == null)
                return -1;
            return plugin.GetCollectedArtworkQuality();
        }
    }
}

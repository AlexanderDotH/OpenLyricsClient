using OpenLyricsClient.Backend.Plugins;
using OpenLyricsClient.Backend.Structure.Song;
using System.Linq;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Collector.Artwork.Providers.Plugin
{
    internal class PluginArtworkCollector : IArtworkCollector
    {
        public string CollectorName()
        {
            return "Plugin";
        }

        async public Task<Structure.Artwork.Artwork> GetArtwork(SongResponseObject songResponseObject)
        {
            Structure.Artwork.Artwork collectedData = new Structure.Artwork.Artwork();
            foreach (IPlugin plugin in Core.INSTANCE.PluginManager.GetPluginsByScope(PluginScope.ArtworkCollector).OrderByDescending((IPlugin plugin) => plugin.GetCollectedArtworkQuality()))
            {
                Structure.Artwork.Artwork? data = await plugin.CollectArtwork(songResponseObject);
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
            else
                return plugin.GetCollectedArtworkQuality();
        }
    }
}

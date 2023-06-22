namespace OpenLyricsClient.Logic.Settings.Sections.Plugins
{
    internal class Structure
    {
        internal class PluginStructure
        {
            public string Name { get; set; } = "";
            public string Path { get; set; } = "";
            public bool Enabled { get; set; } = false;
            public Dictionary<string, string> Settings { get; set; } = new Dictionary<string, string>();
        }

        public List<PluginStructure> Plugins { get; set; } = new List<PluginStructure>();
    }
}

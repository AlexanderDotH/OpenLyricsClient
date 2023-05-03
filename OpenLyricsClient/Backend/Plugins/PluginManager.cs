using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Plugins
{
    internal class PluginManager
    {
        private List<IPlugin> _plugins = new List<IPlugin>();

        private readonly string pluginsFolder;

        internal PluginManager(string workingDirectory)
        {
            pluginsFolder = Path.Join(workingDirectory, "Plugins");
            Directory.CreateDirectory(pluginsFolder);
        }

        public void LoadPlugins()
        {
            _plugins.Clear();

            DirectoryInfo pluginDirectory = new DirectoryInfo(pluginsFolder);

            if (!pluginDirectory.Exists)
                pluginDirectory.Create();
    
            var pluginFiles = Directory.GetFiles(pluginsFolder, "*.dll");

            foreach (var file in pluginFiles)
            {
                Assembly asm = Assembly.LoadFrom(file);
                var types = asm.GetTypes().Where(t => t.GetInterfaces().Where(i => i.FullName == typeof(IPlugin).FullName).Any());

                foreach (var type in types)
                {
                    if (type.FullName != null && asm.CreateInstance(type.FullName) is IPlugin plugin)
                        _plugins.Add(plugin);
                }
            }
        }

        public List<IPlugin> GetPlugins() => _plugins;
    }
}

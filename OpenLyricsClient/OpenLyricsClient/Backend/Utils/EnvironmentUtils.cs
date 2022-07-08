using System;
using System.Linq;

namespace OpenLyricsClient.Backend.Utils
{
    public class EnvironmentUtils
    {
        public static bool IsDebugLogEnabled()
        {
            return Environment.GetCommandLineArgs().Contains("--enable-command-output");
        }
    }
}

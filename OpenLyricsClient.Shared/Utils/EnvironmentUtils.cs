using System;
using System.Linq;

namespace OpenLyricsClient.Shared.Utils
{
    public class EnvironmentUtils
    {
        public static bool IsDebugLogEnabled()
        {
            return System.Environment.GetCommandLineArgs().Contains("--enable-command-output");
        }
    }
}

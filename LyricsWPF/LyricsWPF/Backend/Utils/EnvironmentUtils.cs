using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Utils
{
    public class EnvironmentUtils
    {
        public static bool IsDebugLogEnabled()
        {
            return Environment.GetCommandLineArgs().Contains("--enable-command-output");
        }
    }
}

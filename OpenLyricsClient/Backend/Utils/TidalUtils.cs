using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Utils
{
    public class TidalUtils
    {
        public static bool IsTidalRunning()
        {
            return ProcessUtils.IsProcessRunning("TIDAL");
        }

        public static Process FindTidalProcess()
        {
            Process[] processes = Process.GetProcessesByName("TIDAL");

            for (int i = 0; i < processes.Length; i++)
            {
                Process p = processes[i];

                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    return p;
                }
            }

            return null;
        }
    }
}

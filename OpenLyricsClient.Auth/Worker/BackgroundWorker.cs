using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenLyricsClient.Auth.Worker;

public class BackgroundWorker
{
    public BackgroundWorker()
    {
       Task.Factory.StartNew(DoWork, TaskCreationOptions.LongRunning);
    }

    // TODO: Doesnt work idk why want to fix it later
    private async Task DoWork()
    {
        while (true)
        {
            await Task.Delay(1000);
            
            Process p = Process.GetProcessById(Program.WebViewConfiguration.ParentID);

            if (p == null)
                Environment.Exit(0);
            
            if (p.HasExited)
                Environment.Exit(0);
        }
    }
}
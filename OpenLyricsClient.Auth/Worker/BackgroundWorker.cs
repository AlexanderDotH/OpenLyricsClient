using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Org.BouncyCastle.Bcpg.Sig;

namespace OpenLyricsClient.Auth.Worker;

public class BackgroundWorker
{
    private long _expirationDate;
    
    public BackgroundWorker()
    {
        this._expirationDate = DateTimeOffset.Now.AddMinutes(5).ToUnixTimeMilliseconds();
        
        Task.Factory.StartNew(DoWork, TaskCreationOptions.LongRunning);
    }

    private async Task DoWork()
    {
        while (DateTimeOffset.Now.ToUnixTimeMilliseconds() < this._expirationDate)
        {
            await Task.Delay(1000);
            
            Process p = Process.GetProcessById(Program.WebViewConfiguration.ParentID);

            if (p == null)
                Environment.Exit(0);
            
            if (p.HasExited)
                Environment.Exit(0);
        }
        
        Environment.Exit(0);
    }
}
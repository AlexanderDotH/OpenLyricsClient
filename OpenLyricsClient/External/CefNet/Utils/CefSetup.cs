using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Channels;
using CefNet;
using OpenLyricsClient.Backend.Debugger;

namespace OpenLyricsClient.External.CefNet.Utils;

public class CefSetup
{
    private Debugger<CefSetup> _debugger;
    private CefNetApplication _cefNetApplication;

    public CefSetup()
    {
        this._debugger = new Debugger<CefSetup>(this);
    }

    public void SetupCef()
    {
        try
        {
            CefSettings settings = new CefSettings();
            settings.NoSandbox = true;
            settings.MultiThreadedMessageLoop = true;
            settings.WindowlessRenderingEnabled = true;
            settings.LocalesDirPath = string.Format("{0}\\CefBinaries\\{1}\\Resources\\locales", GetExecutionPath(), GetOSIDentifier());
            settings.ResourcesDirPath = string.Format("{0}\\CefBinaries\\{1}\\Resources", GetExecutionPath(), GetOSIDentifier());
            settings.LogSeverity = CefLogSeverity.Warning;
            settings.UncaughtExceptionStackSize = 8;

            CefNetImplementation cefNetApplication = new CefNetImplementation();
            cefNetApplication.Initialize(string.Format("{0}\\CefBinaries\\{1}\\Release", GetExecutionPath(), GetOSIDentifier()), settings);

            this._cefNetApplication = cefNetApplication;
            
            this._debugger.Write("Cef initialized!", DebugType.INFO);
        }
        catch (Exception e)
        {
            this._debugger.Write("Cannot intiialize cef " + e.Message, DebugType.FATAL);
        }
    }

    private string GetOSIDentifier()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86";
        } 
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux-x64";
        }

        throw new Exception("Cannot identify OS");
    }

    private string GetExecutionPath()
    {
        return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
    }

    public CefNetApplication CefNetApplication
    {
        get => _cefNetApplication;
    }
}
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using CefNet;

namespace OpenLyricsClient.External.CefNet;

class CefNetImplementation : CefNetApplication
{
    protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
    {

        Debug.WriteLine("ChromiumWebBrowser_OnBeforeCommandLineProcessing");
        Debug.WriteLine(commandLine.CommandLineString);

        //commandLine.AppendSwitchWithValue("proxy-server", "127.0.0.1:8888");

        commandLine.AppendSwitch("ignore-certificate-errors");
        commandLine.AppendSwitchWithValue("remote-debugging-port", "9222");
			
        //enable-devtools-experiments
        commandLine.AppendSwitch("enable-devtools-experiments");

        //e.CommandLine.AppendSwitchWithValue("user-agent", "Mozilla/5.0 (Windows 10.0) WebKa/" + DateTime.UtcNow.Ticks);

        //("force-device-scale-factor", "1");

        commandLine.AppendSwitch("disable-gpu");
        commandLine.AppendSwitch("disable-gpu-compositing");
        commandLine.AppendSwitch("disable-gpu-vsync");

        commandLine.AppendSwitch("enable-begin-frame-scheduling");
        commandLine.AppendSwitch("enable-media-stream");

        commandLine.AppendSwitchWithValue("enable-blink-features", "CSSPseudoHas");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            commandLine.AppendSwitch("no-zygote");
            commandLine.AppendSwitch("no-sandbox");
        }
        
        base.OnBeforeCommandLineProcessing(processType, commandLine);
    }

    protected override void OnContextCreated(CefBrowser browser, CefFrame frame, CefV8Context context)
    {
        base.OnContextCreated(browser, frame, context);
        frame.ExecuteJavaScript(@"
{
const newProto = navigator.__proto__;
delete newProto.webdriver;
navigator.__proto__ = newProto;
}", frame.Url, 0);

    }

    public Action<long> ScheduleMessagePumpWorkCallback { get; set; }

    protected override void OnScheduleMessagePumpWork(long delayMs)
    {
        ScheduleMessagePumpWorkCallback(delayMs);
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Accord;
using DevBase.Cryptography.BouncyCastle.Random;
using DevBase.Generics;
using DevBase.IO;
using JetBrains.Annotations;
using JKang.IpcServiceFramework.Hosting;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenLyricsClient.Shared.Communication;
using OpenLyricsClient.Shared.Structure.Access;
using OpenLyricsClient.Shared.Structure.Enum;
using OpenLyricsClient.Shared.Structure.Json;
using OpenLyricsClient.Shared.Utils;
using Org.BouncyCastle.Tls;
using Random = DevBase.Cryptography.BouncyCastle.Random.Random;

namespace OpenLyricsClient.Backend.Authentication;

public class AuthenticationPipe
{
    private string _workingDirectory;

    private AList<(string, object)> _authFlows;
    private AList<(string, Process)> _requestedFlows;
    public AuthenticationPipe()
    {
        this._authFlows = new AList<(string, object)>();
        this._requestedFlows = new AList<(string, Process)>();
        
        this._workingDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Authentication");

        if (!Directory.Exists(this._workingDirectory))
            Directory.CreateDirectory(this._workingDirectory);
    }

    public void Authentication(string flowID, object access)
    {
        this._authFlows.Add((flowID, (object)access));
    }

    public async Task<T> GetToken<T>(string flowID)
    {
        while (true)
        {
            if (HasExited(flowID))
                throw new Exception("Child process got killed");
            
            for (int i = 0; i < this._authFlows.Length; i++)
            {
                (string, object) item = this._authFlows.Get(i);

                if (item.Item1.Equals(flowID))
                    return (T)item.Item2;
            }
        }
    }

    private bool HasExited(string flowID)
    {
        for (int i = 0; i < this._requestedFlows.Length; i++)
        {
            (string, Process) item = this._requestedFlows.Get(i);

            if (item.Item1.Equals(flowID))
                return item.Item2.HasExited;
        }

        return true;
    }
    
    public void KillAuthWindow(string flowID)
    {
        for (int i = 0; i < this._requestedFlows.Length; i++)
        {
            (string, Process) item = this._requestedFlows.Get(i);

            if (item.Item1.Equals(flowID))
                item.Item2.Kill();
        }
    }
    
    public async Task<string> RequestAuthenticationWindow(
        EnumAuthProvider authProvider, 
        string endpoint, 
        string identifier,
        int width, int height)
    {
        FileInfo authFile = FindAuthWindow();

        if (!DataValidator.ValidateData(authFile))
            throw new Exception("AuthFile is missing");
        
        string flowID = new Random().RandomString(5);
        
        ProcessStartInfo process = BuildWindowConfiguration(
            authFile.FullName,
            authProvider, 
            endpoint, 
            identifier, 
            width, 
            height, 
            Core.INSTANCE.InterProcessService.PipeName,
            flowID);
        
        Process p = new Process();
        p.StartInfo = process;
        
        this._requestedFlows.Add((flowID, p));

        p.Start();

        return flowID;
    }

    private ProcessStartInfo BuildWindowConfiguration(
        string filePath,
        EnumAuthProvider authProvider, 
        string endpoint, 
        string identifier,
        int width, int height,
        string pipe,
        string flowID)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();

        StringBuilder builder = new StringBuilder();
        
        builder.Append($"--endpoint=\"{endpoint}\" ");
        builder.Append($"--identifier=\"{identifier}\" ");
        builder.Append($"--provider={(int)authProvider} ");
        builder.Append($"--width={width} ");
        builder.Append($"--height={height} ");
        builder.Append($"--pipe=\"{pipe}\" ");
        builder.Append($"--flowID=\"{flowID}\" ");
        builder.Append($"--parent={Environment.ProcessId}");

        startInfo.Arguments = builder.ToString();
        startInfo.FileName = filePath;

        return startInfo;
    }
    
    private FileInfo FindAuthWindow()
    {
        AList<AFileObject> files = AFile.GetFiles(
            this._workingDirectory, 
            false,
            "*.exe");

        for (int i = 0; i < files.Length; i++)
        {
            AFileObject f = files.Get(i);
            
            if (f.FileInfo.FullName.Contains("OpenLyricsClient.Auth"))
            {
                return f.FileInfo;
            }
        }
        
        return null;
    }

    public void AddAuthflowResult(string flowID, object token)
    {
        this._authFlows.Add((flowID, token));
    }
}
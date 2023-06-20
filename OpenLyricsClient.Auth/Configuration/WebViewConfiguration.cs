using CommandLine;
using OpenLyricsClient.Shared.Structure.Enum;

namespace OpenLyricsClient.Auth.Configuration;

public class WebViewConfiguration
{
    [Option("endpoint", Required = true)]
    public string AuthEndpoint { get; set; }
    
    [Option("identifier", Required = true)]
    public string CompleteIdentifier { get; set; }
    
    [Option("provider", Required = true)]
    public EnumAuthProvider Provider { get; set; }
    
    [Option("width", Required = false)]
    public int Width { get; set; }
    
    [Option("height", Required = false)]
    public int Height { get; set; }
    
    [Option("pipe", Required = true)]
    public string Pipe { get; set; }
    
    [Option("flowID", Required = true)]
    public string Flow { get; set; }

    [Option("parent", Required = true)]
    public int ParentID { get; set; }
}
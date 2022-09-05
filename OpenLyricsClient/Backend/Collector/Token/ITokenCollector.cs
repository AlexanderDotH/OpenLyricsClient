using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLyricsClient.Backend.Collector.Token
{
    public interface ITokenCollector
    {
        Task CollectToken();
    }
}

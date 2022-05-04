using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LyricsWPF.Backend.Events.EventArgs;

namespace LyricsWPF.Backend.Events.EventHandler
{
    public delegate void SongChangedEventHandler(Object sender, SongChangedEventArgs songChangedEvent);
}

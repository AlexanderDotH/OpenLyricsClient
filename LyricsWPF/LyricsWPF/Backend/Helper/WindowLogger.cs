using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevBase.Generic;
using LyricsWPF.Backend.Structure;
using LyricsWPF.Backend.Structure.Enum;

namespace LyricsWPF.Backend.Helper
{
    public class WindowLogger
    {
        private GenericList<Window> _lastWindows;

        public WindowLogger()
        {
            this._lastWindows = new GenericList<Window>();

            Core.INSTANCE.TaskRegister.RegisterTask(LogWindowTask(), RegisterTypes.WINDOW_LOGGER);
        }

        private async Task LogWindowTask()
        {

        }



    }
}

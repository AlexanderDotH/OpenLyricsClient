using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LyricsWPF.Backend.Structure
{
    public class Window
    {

        private string _windowName;

        public Window(string windowName)
        {
            _windowName = windowName;
        }

        public string WindowName
        {
            get => _windowName;
        }
    }
}

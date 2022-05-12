using System;
using System.Linq;

namespace LyricsWPF.Backend.Debug
{
    class Debugger<T>
    {
        private T _type;

        public Debugger(T type)
        {
            this._type = type;
        }

        public void Write(Exception exception)
        {
            this.Write(exception.Message, DebugType.ERROR);
        }

        public void Write(string message, DebugType debugType)
        {
            if (!Environment.GetCommandLineArgs().Contains("--enable-command-output"))
                return;

            switch (debugType)
            {
                case DebugType.INFO:
                {
                    Print(message, debugType);
                    break;
                }
                case DebugType.DEBUG:
                {
                    if (Core.DEBUG_MODE)
                        Print(message, debugType);
                    break;
                }
                case DebugType.ERROR:
                {
                    Print(message, debugType);
                    break;
                }

            }
        }

        private void Print(string message, DebugType debugType)
        {
            Console.WriteLine(string.Format("{0} : {2} : {1}", this._type.GetType().Name , message, debugType.ToString()));
        }
    }
}

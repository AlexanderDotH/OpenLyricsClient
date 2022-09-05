using System;
using System.Diagnostics;
using OpenLyricsClient.Backend.Utils;

namespace OpenLyricsClient.Backend.Debugger
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
            if (!EnvironmentUtils.IsDebugLogEnabled())
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
            Debug.WriteLine(string.Format("{0} : {2} : {1}", this._type.GetType().Name, message, debugType.ToString()));
        }
    }
}

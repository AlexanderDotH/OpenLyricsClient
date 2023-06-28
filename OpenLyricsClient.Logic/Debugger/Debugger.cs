using System.Diagnostics;
using OpenLyricsClient.Shared.Utils;

namespace OpenLyricsClient.Logic.Debugger
{
    public class Debugger<T>
    {
        private T _type;
         
        public Debugger(T type)
        {
            this._type = type;
        }

        public void Write(Exception exception)
        {
            this.Write(exception.Message, DebugType.ERROR);
            Core.INSTANCE.DebugHandler.Write(exception, this._type);
        }

        public void Write(string message, DebugType debugType)
        {
            Print(message, debugType);
        }

        private void Print(string message, DebugType debugType)
        {
            if (!EnvironmentUtils.IsDebugLogEnabled())
                Debug.WriteLine(string.Format("{3} : {0} : {2} : {1}", this._type.GetType().Name, message, debugType.ToString(), DateTime.Now.TimeOfDay.ToString()));
            
            Core.INSTANCE.DebugHandler.Write(message, debugType, this._type);
        }
    }
}

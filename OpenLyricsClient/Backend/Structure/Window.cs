namespace OpenLyricsClient.Backend.Structure
{
    public class Window
    {
        private string _windowTitle;
        private string _processName;

        public Window(string windowTitle, string processName)
        {
            _windowTitle = windowTitle;
            _processName = processName;
        }

        public string WindowTitle
        {
            get => _windowTitle;
        }

        public string ProcessName
        {
            get => _processName;
        }
    }
}

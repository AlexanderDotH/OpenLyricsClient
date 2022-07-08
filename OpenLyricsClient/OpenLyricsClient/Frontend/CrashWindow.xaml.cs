using System;
using System.Reflection;
using System.Text;
using System.Windows;
using OpenLyricsClient.Backend;

namespace OpenLyricsClient.Frontend
{
    /// <summary>
    /// Interaktionslogik für CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : System.Windows.Window
    {

        public CrashWindow()
        {
            InitializeComponent();
        }

        public CrashWindow(string crashMessage) : this()
        {

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Product-Name: " + Assembly.GetExecutingAssembly().GetName().Name);
            builder.AppendLine("Product-Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
            builder.AppendLine("-----------------------------------------");
            builder.Append("Reason: \n" + crashMessage);

            this.CrashLog.Text = builder.ToString();
            Core.INSTANCE.DisposeEverything();
        }

        private void CopyReportBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(this.CrashLog.Text);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Environment.Exit(0);
        }
    }
}

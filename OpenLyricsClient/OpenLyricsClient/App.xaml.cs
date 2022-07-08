using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenLyricsClient.Backend;
using OpenLyricsClient.Frontend;

namespace OpenLyricsClient
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            StringBuilder exception = new StringBuilder();
            exception.AppendLine(e.Exception.Message);
            exception.AppendLine("------------");
            exception.AppendLine(e.Exception.Source);
            exception.AppendLine("------------");
            exception.AppendLine("At: " + e.Exception.TargetSite.Name);
            exception.AppendLine("------------");
            exception.AppendLine(e.Exception.ToString());

            CrashWindow crashWindow = new CrashWindow(exception.ToString());
            crashWindow.Show();
            e.Handled = true;
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            new Core();
        }
    }
}

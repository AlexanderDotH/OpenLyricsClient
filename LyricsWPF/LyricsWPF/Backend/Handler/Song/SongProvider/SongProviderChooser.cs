using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using DevBase.Generic;

namespace LyricsWPF.Backend.Handler.Song.SongProvider
{
    class SongProviderChooser
    {
        public SongProviderChooser() {}

        public EnumSongProvider GetSongProvider()
        {
            return EnumSongProvider.SPOTIFY;
        }

        private bool IsApplicationInForeground(string application)
        {
            if (application.Contains("."))
                application = application.Split('.')[0];

            bool isInForeground = false;

            GenericList<Process> p = new GenericList<Process>(Process.GetProcessesByName(application));

            p.ForEach(t =>
            {
            });

        }
    }
}

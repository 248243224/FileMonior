using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient
{
    public class Entry
    {
        static Config _config;
        IDirectoryMonitor _monitor;
        public Entry(Config config)
        {
            _config = config;
            if (_monitor == null)
                _monitor = DirectoryMonitor.GetInstance(_config);
        }

        public void Run()
        {
            _monitor.Start(DirectoryListener.GetInstance());
        }
        public void Stop()
        {
            _monitor.Stop();
        }
    }
}

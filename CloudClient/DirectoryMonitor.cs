using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudClient;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace CloudClient
{
    public class DirectoryMonitor : IDirectoryMonitor
    {
        private static bool _isStop = false;
        private static DirectoryMonitor _entry;
        private static Config _config;
        static object _objectLock = new object();
        private DirectoryMonitor() { }
        public void Start(IDirectoryMonitorListener listener)
        {
            try
            {
                Trace.TraceInformation("DirectoryMonitor:monitor started...");
                string ignoreFolder = Path.Combine(_config.UnPackDestination, ".acp");
                //set ignore folder
                FileMonitorDllImport.SetExcludeDirectory(ignoreFolder);
                _isStop = false;
                if (!Directory.Exists(_config.DirectoryToMonitor))
                    Directory.CreateDirectory(_config.DirectoryToMonitor);
                if (!Directory.Exists(_config.PackageDestination))
                    Directory.CreateDirectory(_config.PackageDestination);
                FileMonitorDllImport.Start(_config.DirectoryToMonitor);
                FileMonitorDllImport.Start(_config.PackageDestination);
                _config.LatestCommitId = GetLatestCommitId(_config.UnPackDestination);
                while (!_isStop)
                {
                    string updatedValue = Marshal.PtrToStringAnsi(FileMonitorDllImport.GetOutput());
                    if (!string.IsNullOrWhiteSpace(updatedValue))
                        listener.OnUpdated(updatedValue, _config);
                    Thread.Sleep(20);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("DirectoryMonitor:" + ex.Message);
            }
        }
        public void Stop()
        {
            Trace.TraceInformation("monitor stopped...");
            _isStop = true;
            FileMonitorDllImport.Stop();
        }

        public static DirectoryMonitor GetInstance(Config config)
        {
            _config = config;
            if (_entry == null)
            {
                lock (_objectLock)
                {
                    if (_entry == null)
                        _entry = new DirectoryMonitor();
                }
            }
            return _entry;
        }

        string GetLatestCommitId(string unPackDestination)
        {
            try
            {
                string commitFilePath = Path.Combine(unPackDestination, ".acp", "commits");
                string latestCommitId = File.ReadLines(commitFilePath).LastOrDefault().Split('-').LastOrDefault();
                return latestCommitId;
            }
            catch
            {
                return "00000000000000000000000000000000";
            }
        }

    }
}

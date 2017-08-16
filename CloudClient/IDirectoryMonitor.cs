using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient
{
    public interface IDirectoryMonitor
    {
        void Start(IDirectoryMonitorListener listener);
        void Stop();
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient
{
    public class FileMonitorDllImport
    {

        #region FileMonitorDll
        [DllImport("FileMonitorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetExcludeDirectory(string dir);

        [DllImport("FileMonitorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Start(string dir);

        [DllImport("FileMonitorDll.dll", EntryPoint = "OutPut", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetOutput();

        [DllImport("FileMonitorDll.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Stop();
        #endregion
    }
}

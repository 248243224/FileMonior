using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CloudClient.Tester
{
    public class program
    {
        public static void Main()
        {
            //Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //Trace.AutoFlush = true;
            //Trace.Indent();

            Config config = new Config
            {
                KCPVersion = "1.0.0",
                DirectoryToMonitor = @"F:\CheckForUpdateTest\files",
                PackageDestination = @"F:\CheckForUpdateTest\packages",
                UnPackDestination = @"F:\CheckForUpdateTest\files",
                UpdatedFilesProcessInterval = 1 * 1000 * 30//30 seconds
            };

            var entry = new Entry(config);
            entry.Run();
            Console.ReadKey();
        }
    }
}

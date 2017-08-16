using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient
{
    public class Config
    {
        /// <summary>
        /// the interval time to commit changes
        /// </summary>
        public int UpdatedFilesProcessInterval { get; set; }
        /// <summary>
        /// the directory to watch
        /// </summary>
        public string DirectoryToMonitor { get; set; }
        /// <summary>
        /// the loction of updated files
        /// </summary>
        public string PackageDestination { get; set; }
        /// <summary>
        /// the loction of package decompression
        /// </summary>
        public string UnPackDestination { get; set; }
        /// <summary>
        /// kcp version
        /// </summary>
        public string KCPVersion { get; set; }
        /// <summary>
        /// the latest commit id
        /// </summary>
        public string LatestCommitId { get; set; }
    }
}

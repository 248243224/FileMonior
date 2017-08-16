using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient
{
    public class Commit
    {
        /// <summary>
        /// the identifier of the package.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        /// <summary>
        /// if the package contain the changed files only, it is the id of parent changeset,
        /// if it is a full package, the value of parent should be 00000000000000000000000000000000.
        /// </summary>
        public string Parent { get; set; }
        /// <summary>
        /// TBD.
        /// </summary>
        public string Gourp { get; set; }
        /// <summary>
        /// the computer name to create the package. this string must be contained within a <![CDATA[]]> block. 
        /// </summary>
        public string WorkStation { get; set; }
        /// <summary>
        /// the date time to create the package. Format: YYYY-MM-DDThh:mm:ss.sTZD (eg 2017-07-16T19:20:30.45+01:00)
        /// </summary>
        public DateTime DateTime { get; set; }
        /// <summary>
        /// – the directory name of the root directory for the setup files . this string must be contained within a <![CDATA[]]> block. 
        /// </summary>
        public string BaseDirectory { get; set; }
        /// <summary>
        /// the version of Alaris Capture Pro.
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// all the objects are addressed in the <objects> block, a object take some data of the setup file. 
        /// e.g. file name, checksum and the target folder for extraction 
        /// </summary>
        public List<Object> Objects { get; set; }
    }

}

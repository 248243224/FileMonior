using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudClient
{
    [Serializable]
    public class Object
    {
        /// <summary>
        /// the identifier of the object with 32-bits GUID. We can get a file in the objects folder with the id. 
        /// The content of this object is the actual content of the setup file.  
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        /// <summary>
        /// the checksum of the object which is used to verify the file integrity. (Not implemented)
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// the name of setup file. The file need to be rename as this string when it is copied from object folder to target folder. 
        /// this string must be contained within a <![CDATA[]]> block. 
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// the updated status code of the file.
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// the relative path of the target folder which the file will be copied over. this string must be contained within a <![CDATA[]]> block
        /// </summary>
        public string RelativePath { get; set; }
        /// <summary>
        /// the date time of the file's change
        /// </summary>
        public DateTime UpdatedTime { get; set; }
    }
}

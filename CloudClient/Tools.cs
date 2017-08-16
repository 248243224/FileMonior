using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace CloudClient
{
    public class Tools
    {
        /// <summary>
        /// get the hash code of a file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static String ComputeMD5(String fileName)
        {
            String hashMD5 = String.Empty;
            if (System.IO.File.Exists(fileName))
            {
                using (System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
                    Byte[] buffer = calculator.ComputeHash(fs);
                    calculator.Clear();
                    StringBuilder stringBuilder = new StringBuilder();
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        stringBuilder.Append(buffer[i].ToString("x2"));
                    }
                    hashMD5 = stringBuilder.ToString();
                }
            }
            return hashMD5;
        }

        /// <summary>
        /// get workstation
        /// </summary>
        /// <returns></returns>
        public static string GetWorkStation()
        {
            string domainUsername = string.Empty;
            try
            {
                // Define WMI scope to look for the Win32_ComputerSystem object
                ManagementScope ms = new ManagementScope("\\\\.\\root\\cimv2");
                ms.Connect();
                ObjectQuery query = new ObjectQuery
                        ("SELECT * FROM Win32_ComputerSystem");
                ManagementObjectSearcher searcher =
                        new ManagementObjectSearcher(ms, query);
                // This loop will only run at most once.
                foreach (ManagementObject mo in searcher.Get())
                {
                    // Extract the username
                    domainUsername = mo["UserName"].ToString();
                }
            }
            catch { }
            return domainUsername;
        }
        public class XmlUtil
        {
            #region Deserialize
            /// <summary>
            /// Deserialize
            /// </summary>
            /// <param name="type"></param>
            /// <param name="xml"></param>
            /// <returns></returns>
            public static object Deserialize(Type type, string xml)
            {
                try
                {
                    using (StringReader sr = new StringReader(xml))
                    {
                        XmlSerializer xmldes = new XmlSerializer(type);
                        return xmldes.Deserialize(sr);
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            /// <summary>
            /// Deserialize
            /// </summary>
            /// <param name="type"></param>
            /// <param name="xml"></param>
            /// <returns></returns>
            public static object Deserialize(Type type, Stream stream)
            {
                XmlSerializer xmldes = new XmlSerializer(type);
                return xmldes.Deserialize(stream);
            }
            #endregion

            #region Serializer
            /// <summary>
            /// Serializer
            /// </summary>
            /// <param name="type"></param>
            /// <param name="obj"></param>
            /// <returns></returns>
            public static string Serializer(Type type, object obj)
            {
                MemoryStream Stream = new MemoryStream();
                XmlSerializer xml = new XmlSerializer(type);
                try
                {
                    xml.Serialize(Stream, obj);
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
                Stream.Position = 0;
                StreamReader sr = new StreamReader(Stream);
                string str = sr.ReadToEnd();
                sr.Dispose();
                Stream.Dispose();
                return str;
            }

            #endregion
        }
    }
}

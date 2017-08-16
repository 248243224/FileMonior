using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;
using System.Reflection;
using System.Linq.Expressions;
using System.IO;
using System.IO.Compression;

namespace CloudClient
{
    public class DirectoryListener : IDirectoryMonitorListener
    {

        private static DirectoryListener _entry;
        static object _objectLock = new object();
        static List<CloudClient.Object> _objects;
        static System.Threading.Timer _timer;
        static object _onUpdatedLock = new object();
        static object _updatedFilesProcessLock = new object();
        Config _config;
        private DirectoryListener() { }
        public static DirectoryListener GetInstance()
        {
            if (_entry == null)
            {
                lock (_objectLock)
                {
                    if (_entry == null)
                        _entry = new DirectoryListener();
                }
            }
            return _entry;
        }
        /// <summary>
        /// when a file changed 
        /// </summary>
        /// <param name="updateInfo"></param>
        /// <param name="config"></param>
        public void OnUpdated(string updateInfo, Config config)
        {
            lock (_onUpdatedLock)
            {
                if (_config == null)
                    _config = config;
                if (_objects == null)
                    _objects = new List<CloudClient.Object>();
                StartUpdatedFilesProcess();
                //
                if (updateInfo.Contains(config.DirectoryToMonitor))
                {
                    Trace.TraceInformation("DirectoryListener::OnUpdated:Begin to pack changed files");
                    updateInfo = updateInfo.Replace(config.DirectoryToMonitor, "");
                    UpdateInfoToObjects(updateInfo);
                }
                //
                else if (updateInfo.Contains(config.PackageDestination))
                {
                    Trace.TraceInformation("DirectoryListener::OnUpdated:Begin to unpack zip file");
                    updateInfo = updateInfo.Replace(config.PackageDestination, "");
                    _config = config;
                    StartPackageProcess(updateInfo);
                }
            }
        }
        /// <summary>
        /// begin to process the changed files
        /// </summary>
        void StartUpdatedFilesProcess()
        {
            if (_timer == null)
                _timer = new System.Threading.Timer(UpdatedFilesProcess, null, _config.UpdatedFilesProcessInterval, _config.UpdatedFilesProcessInterval);
        }
        /// <summary>
        /// changed files info to objects
        /// </summary>
        /// <param name="updateInfo"></param>
        void UpdateInfoToObjects(string updateInfo)
        {
            try
            {
                string relativePath = updateInfo.Split(':')?.LastOrDefault().TrimStart('\\');
                string filePath = Path.Combine(_config.DirectoryToMonitor, relativePath);
                string fileName = relativePath.Split('\\').LastOrDefault();
                string updateType = updateInfo.Split(':')?.FirstOrDefault();
                //to do:
                lock (_objects)
                {
                    _objects.Add(new CloudClient.Object()
                    {
                        Hash = Tools.ComputeMD5(filePath),
                        Name = fileName,
                        Status = updateType.Contains("added") ?//This is a file you just added
                        "A" : updateType.Contains("modified") ?//You have modified this file. 
                        "M" : updateType.Contains("removed") ?//The file is removed.
                        "R" : "?",//It's not a file in our setup. Capture pro knows nothing about this file
                        RelativePath = relativePath,
                        UpdatedTime = DateTime.Now
                    });
                }

            }
            catch (Exception ex)
            {
                Trace.TraceError("DirectoryListener::UpdateInfoToObjects:" + ex.Message);
            }
        }
        /// <summary>
        /// thread of changed file's processing
        /// </summary>
        /// <param name="state"></param>
        void UpdatedFilesProcess(object state)
        {
            Trace.TraceInformation("DirectoryListener::UpdatedFilesProcess:Begin to process updated files");
            //to do
            lock (_updatedFilesProcessLock)
            {
                lock (_objects)
                {
                    if (_objects.Count > 0)
                    {
                        try
                        {
                            //process the objects
                            _objects = _objects.GroupBy(f => f.RelativePath, (key, group) => group.OrderByDescending(g => g.UpdatedTime).FirstOrDefault()).ToList();
                            //generate the package.xml
                            string commitsRootDirectory = Path.Combine(Path.GetTempPath(), "AcpCommits");
                            Commit commit = new Commit()
                            {
                                Parent = _config.LatestCommitId,
                                Gourp = "imss",
                                WorkStation = $"<![CDATA[{Tools.GetWorkStation()}]]>",
                                DateTime = DateTime.Now,
                                // BaseDirectory = $"<![CDATA[{Config.UnPackDestination}]]>",
                                BaseDirectory = _config.UnPackDestination,
                                Version = _config.KCPVersion,
                                Objects = _objects
                            };
                            //set new latest commit id
                            _config.LatestCommitId = commit.Id;
                            string commitXmlString = Tools.XmlUtil.Serializer(typeof(Commit), commit);
                            //create the directorys of commits
                            string commitInfoDirectoryName = string.Concat(commit.Parent, "-", commit.Id);
                            string commitInfoDirectoryPath = Path.Combine(commitsRootDirectory, commitInfoDirectoryName);
                            Directory.CreateDirectory(commitInfoDirectoryPath);
                            //create package.xml
                            string packageXmlPath = Path.Combine(commitInfoDirectoryPath, "package.xml");
                            using (FileStream packageXml = new FileStream(packageXmlPath, FileMode.Create))
                            {
                                using (StreamWriter writer = new StreamWriter(packageXml, Encoding.UTF8))
                                {
                                    writer.WriteLine(commitXmlString);
                                }
                            }
                            //create objects directory and files
                            _objects.ForEach(o =>
                            {
                                if (o.Status.Equals("M"))
                                {
                                    string folderName = o.Id.Substring(0, 2);
                                    string folderPath = Path.Combine(commitInfoDirectoryPath, folderName);
                                    string fileName = o.Id.Substring(2);
                                    string desFilePath = Path.Combine(folderPath, fileName);
                                    string sourceFilePath = Path.Combine(_config.DirectoryToMonitor, o.RelativePath);
                                    if (!Directory.Exists(folderPath))
                                        Directory.CreateDirectory(folderPath);
                                    if (File.Exists(sourceFilePath))
                                        File.Copy(sourceFilePath, desFilePath);
                                }
                            });
                            //pack the updated files
                            string zipFile = Path.Combine(_config.PackageDestination, string.Concat(commitInfoDirectoryName, ".package"));
                            if (!Directory.Exists(_config.PackageDestination))
                                Directory.CreateDirectory(_config.PackageDestination);
                            ZipFile.CreateFromDirectory(commitInfoDirectoryPath, zipFile);
                            Directory.Delete(commitsRootDirectory, true);
                            _objects.Clear();
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("DirectoryListener::UpdatedFilesProcess:" + ex.Message);
                        }
                    }
                    else
                        Trace.TraceInformation("DirectoryListener::UpdatedFilesProcess:There is noting changed");
                }
            }
        }
        /// <summary>
        /// unpacking packages to destination
        /// </summary>
        /// <param name="updateInfo"></param>
        void StartPackageProcess(string updateInfo)
        {
            int retries = 3;
            retry:
            try
            {
                string fileLocation = updateInfo.Split(':')?.LastOrDefault().TrimStart('\\');
                string updateType = updateInfo.Split(':')?.FirstOrDefault();
                string folderName = fileLocation.Split('.').FirstOrDefault().TrimStart('\\');
                if (updateType.Contains("added") && fileLocation.EndsWith(".package"))
                {
                    string rootPath = Path.Combine(_config.UnPackDestination, ".acp");
                    if (!Directory.Exists(rootPath))
                    {
                        DirectoryInfo di = Directory.CreateDirectory(rootPath);
                        di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                    }
                    string sourceFile = Path.Combine(_config.PackageDestination, fileLocation);
                    string desFile = Path.Combine(rootPath, folderName);
                    ZipFile.ExtractToDirectory(sourceFile, desFile);
                    //apply the changes
                    string PackageXmlPath = Path.Combine(desFile, "package.xml");
                    string packageXmlString = File.ReadAllText(PackageXmlPath);
                    Commit commit = Tools.XmlUtil.Deserialize(typeof(Commit), packageXmlString) as Commit;
                    //stop directory monitor
                    DirectoryMonitor.GetInstance(_config).Stop();
                    commit.Objects.ForEach(o =>
                    {
                        if (o.Status.Equals("M"))
                        {
                            string objectfolderName = o.Id.Substring(0, 2);
                            string objectFileName = o.Id.Substring(2);
                            string ojectFilePath = Path.Combine(desFile, objectfolderName, objectFileName);
                            string applyDes = Path.Combine(commit.BaseDirectory, o.RelativePath);
                            string applyFolder = applyDes.Replace(o.Name, "");
                            if (!Directory.Exists(applyFolder))
                                Directory.CreateDirectory(applyFolder);
                            File.Copy(ojectFilePath, applyDes, true);
                        }
                        else if (o.Status.Equals("R"))
                        {
                            string applyDes = Path.Combine(commit.BaseDirectory, o.RelativePath);
                            if (File.Exists(applyDes))
                                File.Delete(applyDes);
                            else if (Directory.Exists(applyDes))
                                Directory.Delete(applyDes, true);
                        }
                    });
                    //update commit file
                    string commitFile = Path.Combine(rootPath, "commits");
                    File.AppendAllText(commitFile, folderName + Environment.NewLine);
                    //stop directory monitor
                    DirectoryMonitor.GetInstance(_config).Start(DirectoryListener.GetInstance());
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("DirectoryListener::StartPackageProcess:" + ex.Message);
                if (retries <= 0) throw;
                retries--;
                goto retry;
            }
        }

    }
}

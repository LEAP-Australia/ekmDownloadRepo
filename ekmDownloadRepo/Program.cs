using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ekmDownloadRepo
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 6)
            {
                Console.Write("ekmDownloadRepo\n(c) LEAP Australia Pty Ltd 2017\n\nUsage:\n");
                Console.Write("ekmDownloadRepo ekmHostAddress ekmUserName ekmUserPassword repoLocation localTargetDestination errorLog\n\n");
                return;
            }
            var hostName = args[0];
            var userName = args[1];
            var password = args[2];
            var rootRepoPath = args[3];
            var rootDiskPath = args[4];
            var failLogFile = args[5];

            try
            {
                using (var ekmMgr = new ekmController(hostName, userName, password))
                {
                    var filePaths = ekmMgr.recursivelyGetAllObjs(rootRepoPath);
                    using (var failedObjFile = new System.IO.StreamWriter(failLogFile, false))
                    {
                        failedObjFile.WriteLine("===================== Failed to Obtain File Info  =====================");

                        foreach (var file in ekmMgr.lastFailed)
                        {
                            failedObjFile.WriteLine(file);
                        }

                        failedObjFile.WriteLine("============================ End of Section ===========================");
                    }

                    var failedToDownload = new List<string>();

                    //List<string> prevDownloadedFiles = getExistingFiles(rootDiskPath);

                    foreach(var filePath in filePaths)
                    {
                        var dirParts = filePath.Split('/');
                        var dirPath = System.IO.Path.Combine(rootDiskPath,
                            System.IO.Path.Combine(dirParts.Take(dirParts.Length - 1).ToArray()));

                        Console.Write("Attempting to download: {0}...", filePath);
                        bool dlSuccess = false;
                        try
                        {
                            dlSuccess = ekmMgr.downloadFile(filePath, dirPath);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("{0}", e);
                        }

                        if (dlSuccess)
                        {
                            Console.WriteLine("Success!");
                            
                        }
                        else
                        {
                            failedToDownload.Add(filePath);
                            Console.WriteLine("Failed!");
                        }
                    }

                    using (var failedObjFile = new System.IO.StreamWriter(failLogFile, true))
                    {
                        failedObjFile.WriteLine("======================= Failed to Download File =======================");

                        foreach (var file in failedToDownload)
                        {
                            failedObjFile.WriteLine(file);
                        }

                        failedObjFile.WriteLine("============================ End of Section ===========================");
                    }
                }
            }
            catch(Exception e)
            {
                Console.Write("Something went wrong. This may help. Good luck\n\n{0}", e.Message);
            }
        }

        private static List<string> getExistingFiles(string parentPath, string rootPath = null)
        {
            if (rootPath == null)
                rootPath = parentPath;

            var repoFiles = new List<string>();
            var fileListing = System.IO.Directory.GetFiles(parentPath);

            var baseParts = rootPath.Split(System.IO.Path.DirectorySeparatorChar);

            var wbPrjFiles = new List<string[]>();

            foreach(var file in fileListing)
            {
                var fileParts = file.Split(System.IO.Path.DirectorySeparatorChar);
                repoFiles.Add("/" + String.Join("/", fileParts.Skip(baseParts.Length)));
                if (fileParts.Last().Split('.').Last().Equals("wbpj", StringComparison.CurrentCultureIgnoreCase))
                    wbPrjFiles.Add(fileParts);
            }

            var wbPrjDir = new List<string>();

            foreach(var file in wbPrjFiles)
            {
                var fileNameParts = file.Last().Split('.');
                var dirName = String.Join(".", fileNameParts.Take(fileNameParts.Length - 1)) + "_files";

                var dirParts = file.Take(file.Length - 1).ToList();
                dirParts.Add(dirName);

                wbPrjDir.Add(String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), dirParts).ToLower());
            }

            foreach(var dir in System.IO.Directory.GetDirectories(parentPath))
            {
                if (!wbPrjDir.Contains(dir.ToLower()))
                    repoFiles.AddRange(getExistingFiles(dir, rootPath));
            }

            return repoFiles;
        }
    }
}

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

                    foreach(var filePath in filePaths)
                    {
                        var dirParts = filePath.Split('/');
                        var dirPath = System.IO.Path.Combine(rootDiskPath,
                            System.IO.Path.Combine(dirParts.Take(dirParts.Length - 1).ToArray()));

                        Console.Write("Attempting to download: {0}...", filePath);
                        if (!ekmMgr.downloadFile(filePath, dirPath))
                        {
                            failedToDownload.Add(filePath);
                            Console.WriteLine("Failed!");
                        }
                        else
                        {
                            Console.WriteLine("Success!");
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
    }
}

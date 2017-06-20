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
            var hostName = args[0];
            var userName = args[1];
            var password = args[2];
            var rootRepoPath = args[3];
            var rootDiskPath = args[4]; 

            try
            {
                using (var ekmMgr = new ekmController(hostName, userName, password))
                {
                    var filePaths = ekmMgr.recursivelyGetAllObjs(rootRepoPath);
                    using(var failedObjFile = new System.IO.StreamWriter("failed.txt", false))
                    {
                        failedObjFile.WriteLine("===========Failed to obtain file list===========");

                        foreach (var file in ekmMgr.lastFailed)
                        {
                            failedObjFile.WriteLine(file);
                        }

                        failedObjFile.WriteLine("===========            END          ===========");
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

                    using (var failedObjFile = new System.IO.StreamWriter("failed.txt", true))
                    {
                        failedObjFile.WriteLine("=========Failed to download file list==========");

                        foreach (var file in failedToDownload)
                        {
                            failedObjFile.WriteLine(file);
                        }

                        failedObjFile.WriteLine("===========            END          ===========");
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine("Something went wrong. {0}", e.Message);
            }
        }
    }
}

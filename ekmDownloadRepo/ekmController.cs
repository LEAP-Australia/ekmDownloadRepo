using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using com.ansys.ingress.client.api;
using com.ansys.ingress.connector.model;
using com.ansys.ingress.connector.model.spec;

namespace ekmDownloadRepo
{
    internal class ekmController : IDisposable
    {
        private Connection conn  = null;
        private DataManager dataMgr;
        private AdministrationManager adminMgr;
        public List<string> lastFailed;

        private string hostName;
        private string userName;
        private string password;

        private bool workerToManagerNeedConnection = false;
        private bool managerToWorkerConnIsValid = false;

        private Thread connShop = null;

        private const uint limitCount = 10;
        private const int managerFreq = 500;
        private const int workerFreq = 2000;
        private const int counterLimit = 7200;

        public ekmController(string hostname, string username, string passwd)
        {
            hostName = hostname;
            userName = username;
            password = passwd;
            workerToManagerNeedConnection = true;

            connShop = new Thread(connectionManager);
            connShop.Start();
            lastFailed = new List<string>();
        }

        private void connectionManager()
        {
            var counter = 0;
            while(true)
            {
                if(workerToManagerNeedConnection)
                {
                    bool connIsGood;

                    try
                    {
                        connIsGood = conn != null && conn.IsOpen() && conn.IsSessionValid() && conn.IsLoggedIn();
                    }
                    catch
                    {
                        connIsGood = false;
                    }

                    if (connIsGood) // && conn.IsLoggedIn() 
                    {
                        managerToWorkerConnIsValid = true;
                        counter = 0;
                    }
                    else
                    {
                        managerToWorkerConnIsValid = false;
                        closeConnection();
                        try
                        {
                            conn = ConnectionFactory.Open(hostName, userName, password);
                            dataMgr = conn.GetDataManager();
                            adminMgr = conn.GetAdministrationManager();
                            managerToWorkerConnIsValid = true;
                            counter = 0;
                        }
                        catch (ConnectionException e)
                        {
                            Console.WriteLine("**** Failed to Connect:\n{0}\nWill Try again", e.Message);
                            counter++;
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("**** Failed to Connect with unknown exception:\n{0}\nOf Type {1}\nWill Try again", e.Message, e.GetType());
                            counter++;
                        }
                    }
                }
                else
                {
                    closeConnection();
                    managerToWorkerConnIsValid = false;
                    break;
                }
                if (counter > counterLimit)
                    throw new Exception("Conn Manager timed out");
                System.Threading.Thread.Sleep(managerFreq);
            }
        }

        public List<string> recursivelyGetAllObjs(string parentPath)
        {
            if (!connShop.IsAlive)
            {
                throw new Exception("Conn Manager has failed");
            }

            while(!managerToWorkerConnIsValid)
            {
                Thread.Sleep(workerFreq);
            }

            var objectPaths = new List<string>();

            var parentObj = dataMgr.FindByPath(parentPath);

            foreach(var childModel in parentObj.Children)
            {
                try
                {
                    var childObj = dataMgr.FindByPath(childModel.Value.Path);
                    if (!childObj.GetTypeName().Contains("Archive") && childObj.Children.Count > 0)
                    {
                        objectPaths.AddRange(recursivelyGetAllObjs(childObj.Path));
                    }
                    else
                    {
                        objectPaths.Add(childObj.Path);
                        Console.WriteLine(childObj.Path);
                    }     
                }
                catch
                {
                    lastFailed.Add(childModel.Value.Path);
                }
               
            }
            return objectPaths;            
        }

        public bool downloadFile(string repoPath, string diskPath, bool furtherCalls=true)
        {
            if (!connShop.IsAlive)
                return false;

            while (!managerToWorkerConnIsValid)
            {
                Thread.Sleep(workerFreq);
            }

            bool successful = false;
            var listener = new FileTransferListener();
            try
            {
                var paths = new List<string>() { repoPath };
                dataMgr.Download(paths, diskPath, true, listener);
                listener.WaitForTransferToEnd();
                successful = true;
            }
            catch(Exception e)
            {
                if(furtherCalls)
                {
                    Console.WriteLine(
                        "\nUnable to download file. Reason\n{0}\n{1}.\nResetting connection",
                        e.Message, e.GetType());
                    closeConnection();
                    Thread.Sleep(workerFreq);
                    successful = downloadFile(repoPath, diskPath, false);
                }
                else
                {
                    Console.WriteLine(
                        "\nUnable to download file. Reason\n{0}\n{1}",
                        e.Message, e.GetType());
                    successful = false;
                }
            }
            return successful;
        }


        private void closeConnection()
        {
            if (conn != null)
            {
                try
                {
                    conn.Close();
                }
                catch
                {
                    conn = null; 
                }
            }
                
            conn = null;
        }

        public void Dispose()
        {
            workerToManagerNeedConnection = false;
            connShop.Join();
        }
    }
}

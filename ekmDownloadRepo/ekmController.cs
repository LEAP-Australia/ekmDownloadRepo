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

        private const uint limitCount = 10;

        public ekmController(string hostname, string username, string passwd)
        {
            hostName = hostname;
            userName = username;
            password = passwd;

            Exception ex= null;

            for(uint i =0; i<limitCount; ++i)
            {
                try
                {
                    createConnection();
                    dataMgr = conn.GetDataManager();
                    adminMgr = conn.GetAdministrationManager();
                    lastFailed = new List<string>();
                    return;
                }
                catch(Exception e)
                {
                    ex = e;
                    Thread.Sleep(5000);
                }
            }

            throw ex;
        }

        private void createConnection()
        {
            conn = ConnectionFactory.Open(hostName, userName, password);
        }

        public List<string> recursivelyGetAllObjs(string parentPath)
        {
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
                        //Console.WriteLine(childObj.Path);
                        objectPaths.Add(childObj.Path);
                    }     
                }
                catch
                {
                    lastFailed.Add(childModel.Value.Path);
                }
               
            }
            return objectPaths;
        }

        public bool downloadFile(string repoPath, string diskPath)
        {
            try
            {
                if (!(conn.IsOpen() && conn.IsLoggedIn() && conn.IsSessionValid()))
                {
                    createConnection();
                }
            }
            catch(Exception e)
            {
                throw e;
            }

            bool successful = true;
            var listener = new FileTransferListener();
            try
            {
                var paths = new List<string>() {repoPath};
                dataMgr.Download(paths, diskPath, true, listener);
                listener.WaitForTransferToEnd();
            }
            catch
            {
                successful = false;
            }
            return successful;
        }

        public void closeConnection()
        {
            if (conn != null)
                conn.Close();
        }

        public void Dispose()
        {
            closeConnection();
        }
    }
}

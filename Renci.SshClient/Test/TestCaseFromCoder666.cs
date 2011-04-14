using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Renci.SshClient;

namespace Test
{
    public class ExternalSystem
    {
        public string IP = "152.54.8.155";
        public string Username = "oleg";
        public string Password = "Maya3kap$";
    }

    public class SFTPClient
    {
        private const int max_sftp_per_server = 10;

        static Thread[] threads = null;
        static public void DoWork(object obj)
        {

            string sFileToUpload = obj as string;
            ExternalSystem system = new ExternalSystem();

            for (int j = 0; j < 1000; j++)
            {

                try
                {
                    SFTPClient.ExecuteSFTPUpload(system, sFileToUpload, Path.GetFileName(sFileToUpload), false, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

        }
        static public void Test()
        {
            threads = new Thread[1];


            byte[] buffer = new byte[10 * 1024];

            for (int i = 0; i < threads.Length; i++)
            {

                string sFileToUpload = string.Format(@"C:\Test\user_test{0}.txt", i);

                using (FileStream oStream = File.Create(sFileToUpload))
                {
                    oStream.Write(buffer, 0, buffer.Length);
                }


                threads[i] = new Thread(SFTPClient.DoWork);

                threads[i].Start(sFileToUpload);
            }
        }

        private class SftpHolder
        {
            private DateTime recyleAfter = DateTime.MinValue;
            private SftpClient sftp = null;
            private Object lockObj = new object();
            private int putcnt = 0;

            public bool HasConnection()
            {
                return sftp != null;
            }

            public void Connect(string server, string username, string password)
            {
                sftp = new SftpClient(server, username, password);
                sftp.OperationTimeout = new TimeSpan(0, 1, 0);
                sftp.Connect();
                putcnt = 0;
                recyleAfter = DateTime.Now.AddMinutes(2);

            }

            public void RecycleIfTime()
            {

                if (DateTime.Now > recyleAfter || putcnt > 100)
                {
                    if (sftp.IsConnected)
                    {
                        sftp.Disconnect();
                    }
                    sftp.Dispose();
                    sftp = null;
                    Thread.Sleep(0); //allow GC a look in 
                }

            }

            public void put(string from, string to)
            {
                using (var fs = new FileStream(from, FileMode.Open))
                {
                    putcnt++;
                    //var beforeUpload = GC.GetTotalMemory(true);
                    sftp.UploadFile(fs, to);
                    //var afterUpload = GC.GetTotalMemory(true);
                    //Console.WriteLine("Difference:" + (afterUpload - beforeUpload));
                }
            }

            public void FreeConnection()
            {
                if (HasConnection())
                {
                    SftpClient tmp = sftp;
                    sftp = null;
                    if (tmp.IsConnected)
                    {
                        try
                        {
                            tmp.Disconnect();
                        }
                        finally
                        {
                        }
                    }
                    tmp.Dispose();
                }
            }

            public bool Lock()
            {
                return Monitor.TryEnter(lockObj);
            }
            public void Unlock()
            {
                Monitor.Exit(lockObj);
            }

        }
        private static Dictionary<string, SftpHolder[]> sftpDictionary = new Dictionary<string, SftpHolder[]>();

        static object lockObj = new object();



        static private void Put(SftpHolder sftpcon, string from, string to)
        {
            sftpcon.put(from, to);
        }


        public static void ExecuteSFTPUpload(ExternalSystem system, string sFileToUpload)
        {
            ExecuteSFTPUpload(system, sFileToUpload, Path.GetFileName(sFileToUpload), false, false);
        }

        public static void ExecuteSFTPUpload(ExternalSystem system, string sFileToUpload, string sRemoteFileFullPath, bool bCreateFlagFile)
        {
            ExecuteSFTPUpload(system, sFileToUpload, sRemoteFileFullPath, bCreateFlagFile, false);
        }

        public static void ExecuteSFTPUpload(ExternalSystem system, string sFileToUpload, string sRemoteFileFullPath, bool bCreateFlagFile, bool overrideSftpFlag)
        {
            string server = system.IP;



            DateTime dtStart = DateTime.Now;

            SftpHolder[] serverCons = null;
            lock (sftpDictionary)
            {
                if (!sftpDictionary.TryGetValue(server, out serverCons))
                {
                    serverCons = new SftpHolder[max_sftp_per_server];
                    for (int i = 0; i < max_sftp_per_server; i++)
                    {
                        serverCons[i] = new SftpHolder();
                    }
                    sftpDictionary.Add(server, serverCons);

                }
            }

            SftpHolder currentCon = null;

            try
            {
                lock (serverCons)
                {
                    DateTime waitUntil = DateTime.Now.AddMinutes(5);
                    while (currentCon == null)
                    {
                        int i = 0;
                        foreach (SftpHolder con in serverCons)
                        {
                            if (con.Lock())//try and lock an unused sftp connection
                            {
                                currentCon = con;       //got one
                                break;
                            }
                            i++;
                        }
                        if (currentCon == null)
                        {
                            if (DateTime.Now < waitUntil)
                                Thread.Sleep(10);           //all sftp sessions are in use so wait
                            else
                                throw new Exception("Timed out waiting for an SFTP connection");
                        }
                    }


                    if (!currentCon.HasConnection())        //is there currently a connected session
                    {

                        currentCon.Connect(server, system.Username, system.Password);
                        dtStart = DateTime.Now;
                    }
                }
                Put(currentCon, sFileToUpload, sRemoteFileFullPath);

                // currentCon.RecycleIfTime();

            }
            catch (Exception ex)
            {
                if (currentCon != null)
                {
                    currentCon.FreeConnection();
                }
                throw new Exception(ex.Message, ex.InnerException);//mep Tamir Jsch exceptions are not serializeable so need to repackage exception for remoting

            }
            finally
            {
                if (currentCon != null) currentCon.Unlock();
                currentCon = null;
            }
        }
    }
}

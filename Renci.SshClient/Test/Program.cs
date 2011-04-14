using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Win32;
using Renci.SshClient;
using Renci.SshClient.Common;
using Renci.SshClient.Sftp;
using System.Collections.Generic;
using Renci.SshClient.Security.Cryptography;
using Renci.SshClient.Tests.Security.Cryptography;

namespace Test
{
    class Program
    {
        static void DisplayMemory()
        {
            Console.WriteLine("Total memory: {0:###,###,###,##0} bytes", GC.GetTotalMemory(true));
        }

        public static byte[] Test1(uint value)
        {
            return BitConverter.GetBytes(12).Reverse().ToArray();
        }

        public static byte[] Test2(uint value)
        {
            return new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF) };
        }

        public static byte[] Test3(int value)
        {
            return new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)(value & 0xFF) };
        }

        static void Main(string[] args)
        {
            var test = new TestAes();
            for (int i = 0; i < 1000; i++)
            {
                test.Test_AES_128_CBC_NoPadding();
            }
            return;
            //TestBlowfish();
            ////TestCrypto();
            //return;

            //    new BlockCipherVectorTest(0, new AesFastEngine(),
            //new KeyParameter(Hex.Decode("80000000000000000000000000000000")),
            //"00000000000000000000000000000000", "0EDD33D3C621E546455BD8BA1418BEC8"),

            //arcfour256,arcfour128,arcfour,rijndael-cbc@lysator.liu.se




            //SFTPClient.Test();
            //while (true)
            //{
            //    Console.WriteLine(string.Format("Memory usage is {0}", System.GC.GetTotalMemory(true)));
            //    Thread.Sleep(1000);
            //}


            //CreateTestFile(@"C:\Test\testfile10mb.bin", 1024 * 10);

            //var connectionInfo = new PrivateKeyConnectionInfo("host", 1234, "username", new PrivateKeyFile(File.OpenRead(@"H:\My Documents\SSHKeys\oleg-centos.edc.renci.org\rsa_pass_key.txt"), "tester")); 
            //var connectionInfo = new PasswordConnectionInfo("host", 1234, "username", "password");

            var connectionInfo = new PasswordConnectionInfo("152.54.9.9", 2222, "oleg", "Maya1kap@");
            //var connectionInfo = new PasswordConnectionInfo("oleg-centos.edc.renci.org", 22, "oleg", "Maya1kap");            

            //var connectionInfo = new PasswordConnectionInfo("oleg-centos.edc.renci.org", 22, "tester", "tester");
            //var connectionInfo = new PasswordConnectionInfo("152.54.8.155", 2222, "oleg", "Maya3kap$");

            using (var ssh = new SshClient(connectionInfo))
            {
                ssh.Connect();
                var result = ssh.RunCommand("ls -l");
                //var input = new MemoryStream(Encoding.ASCII.GetBytes("sudo ufw status\r\nPassword\r\n"));
                ////var input = Console.OpenStandardInput();
                //var shell = ssh.CreateShell(input, Console.Out, Console.Out, "xterm", 80, 24, 800, 600, "");
                //shell.Stopped += delegate(object sender, EventArgs e)
                //{
                //    Console.WriteLine("\nDisconnected...");
                //};
                //shell.Start();
                //Thread.Sleep(1000 * 1000);
                //shell.Stop();
            }


            using (var sftp = new SftpClient(connectionInfo))
            {
                sftp.Connect();

                IEnumerable<SftpFile> old_files = null;

                for (int i = 0; i < 1000; i++)
                {
                    var files = sftp.ListDirectory("");
                    if (old_files != null)
                        Console.WriteLine(old_files.Count());
                    DisplayMemory();
                    old_files = files;

                    //var beforeUpload = GC.GetTotalMemory(true);                   

                    //using (var fs = new FileStream(string.Format(@"C:\Test\user_test0.txt"), FileMode.Open))
                    //{
                    //    sftp.UploadFile(fs, string.Format("user_test{0}.txt", i));
                    //}
                    //var afterUpload = GC.GetTotalMemory(true);
                    //Console.WriteLine("Difference:" + (afterUpload - beforeUpload));
                }
                sftp.Disconnect();
            }

            Console.WriteLine("Finished.");
            Console.ReadKey();


            //using (var ssh = new SshClient(connectionInfo))
            //{
            //    ssh.Connect();
            //    ssh.RunCommand("rm -rf test_*");
            //    ssh.Disconnect();
            //}

            //using (var sftp = new SftpClient(connectionInfo))
            //{
            //    sftp.Connect();

            //    //  Create 10000 directory items
            //    for (int i = 0; i < 10000; i++)
            //    {
            //        sftp.CreateDirectory(string.Format("test_{0}", i));
            //        Debug.WriteLine("Created " + i);
            //    }

            //    var files = sftp.ListDirectory(".");

            //    sftp.Disconnect();
            //}

            for (int i = 0; i < 1; i++)
            {

                DisplayMemory();
                using (var sftp = new SftpClient(connectionInfo))
                {
                    sftp.Connect();
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    sftp.UploadFile(File.OpenRead(@"C:\Test\testfile.bin"), "testfile.bin");
                    //sftp.UploadFile(File.OpenRead(@"C:\Test\testfile10mb.bin"), "testfile.bin");
                    Console.WriteLine(string.Format("Uploaded in {0}", watch.ElapsedMilliseconds));
                    //watch.Restart();
                    //sftp.DownloadFile("testfile.bin", File.Create(@"C:\Test\testfile1.bin"));
                    //Console.WriteLine(string.Format("Downloaded in {0}", watch.ElapsedMilliseconds));
                    watch.Stop();
                }
                DisplayMemory();
            }

            //Console.ReadKey();
            return;

            using (var sftp = new SftpClient(connectionInfo))
            {
                sftp.Connect();


                //sftp.DownloadFile("test2.txt", File.Create(@"C:\Test\test2.txt"));
                //sftp.DownloadFile("test3.txt", File.Create(@"C:\Test\test3.txt"));
                sftp.DownloadFile("test4.txt", File.Create(@"C:\Test\test4.txt"));


                sftp.UploadFile(File.OpenRead(@"C:\VirtualBoxImages\IRODS 2.4.vmdk"), "bigtestfile.bin");
                sftp.ErrorOccurred += delegate(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("Error occured: " + e.GetException().ToString());
        };

                Thread.Sleep(1000 * 60 * 60);

                var list = sftp.ListDirectory("/home/oleg");

                var file = (from f in list
                            where f.Name == "test2.txt"
                            select f).FirstOrDefault();

                file.SetPermissions(555);
                file.SetPermissions(666);
                file.SetPermissions(777);
                file.Delete();

                file.MoveTo("/home/oleg/test1.txt.new");

                file.OthersCanRead = false;
                file.GroupCanRead = false;

                file.UpdateStatus();

                file.OthersCanExecute = true;
                file.GroupCanExecute = true;

                ListCurrentDirectory(sftp);
                sftp.ChangeDirectory("/home");
                ListCurrentDirectory(sftp);
                ListCurrentDirectory(sftp, "oleg/Postgres");
                sftp.ChangeDirectory("..");
                ListCurrentDirectory(sftp);
                sftp.ChangeDirectory("/home/oleg/Postgres/");
                ListCurrentDirectory(sftp);
                sftp.ChangeDirectory("../../tester/");
                ListCurrentDirectory(sftp);
                sftp.ChangeDirectory("/home/");
                ListCurrentDirectory(sftp);
                sftp.ChangeDirectory("/");
                ListCurrentDirectory(sftp);

                sftp.Disconnect();
            }



            DisplayMemory();
            for (int i = 0; i < 10; i++)
            {
                using (var ssh = new SshClient(connectionInfo))
                {
                    ssh.Connect();
                    //for (int i = 0; i < 10; i++)
                    ////while (true)
                    //{
                    //    //using (var result = ssh.RunCommand("cat test1.bin"))
                    using (var result = ssh.RunCommand("ls -l;"))
                    {
                        Console.WriteLine(string.Format("{0}:\tReceived: {1} bytes", DateTime.Now, result.Result.Length));
                    }
                    //    //DisplayMemory();
                    //}
                    ssh.Disconnect();
                    ssh.Connect();
                    using (var result = ssh.RunCommand("ls -l;"))
                    {
                        Console.WriteLine(string.Format("{0}:\tReceived: {1} bytes", DateTime.Now, result.Result.Length));
                    }
                    ssh.Disconnect();
                }
                DisplayMemory();
            }

            //var sftp = new SftpClient(connectionInfo);
            //sftp.Connect();
            //sftp.KeepAliveInterval = TimeSpan.FromSeconds(10);
            ////sftp.ChangePermissions("test.txt", 70707);
            //sftp.ChangeOwner("test.txt", 0);
            //sftp.ChangeGroup("sltest", 1104);

            //sftp.DeleteDirectory("tobelinked");

            //sftp.SymbolicLink("linked/", "tobelinked/");
            //var rresult = sftp.ListDirectory(".");

            //connectionInfo.PasswordExpired += delegate(object sender, AuthenticationPasswordChangeEventArgs e)
            //                                    {
            //                                        e.NewPassword = "123456";
            //                                    };

            //var connectionInfo = new KeyboardInteractiveConnectionInfo("152.54.8.155", 2222, "oleg");
            //var connectionInfo = new KeyboardInteractiveConnectionInfo("host", "username");
            //connectionInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
            //{
            //    Console.WriteLine(e.Instruction);

            //    foreach (var prompt in e.Prompts)
            //    {
            //        Console.WriteLine(prompt.Request);
            //        prompt.Response = Console.ReadLine();
            //    }
            //};

            //using (var client = new SshClient(connectionInfo))
            //{
            //    client.Connect();
            //    var cmd = client.CreateCommand("date");
            //    cmd.CommandTimeout = TimeSpan.FromSeconds(10);
            //    cmd.Execute();
            //    Console.WriteLine(cmd.Result);
            //    cmd.Execute("ls -l");
            //    Console.WriteLine(cmd.Result);
            //    client.Disconnect();
            //}

            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                //var port1 = client.AddForwardedPort<ForwardedPortRemote>("152.54.8.155", 8081, "www.renci.org", 80);
                //var port1 = client.AddForwardedPort<ForwardedPortRemote>(8081, "www.renci.org", 80);
                //var port1 = client.AddForwardedPort<ForwardedPortLocal>("152.54.8.108", 8084, "www.renci.org", 80);
                var port1 = client.AddForwardedPort<ForwardedPortRemote>(8081, "www.renci.org", 80);
                port1.Exception += delegate(object sender, ExceptionEventArgs e)
                {
                    Console.WriteLine(e.Exception.ToString());
                };
                port1.RequestReceived += delegate(object sender, PortForwardEventArgs e)
                {
                    Console.WriteLine(e.OriginatorHost + ":" + e.OriginatorPort);
                };
                port1.Start();

                var port2 = client.AddForwardedPort<ForwardedPortLocal>("152.54.8.108", 8084, "www.renci.org", 80);

                port2.Exception += delegate(object sender, ExceptionEventArgs e)
                {
                    Console.WriteLine(e.Exception.ToString());
                };
                port2.RequestReceived += delegate(object sender, PortForwardEventArgs e)
                {
                    Console.WriteLine(e.OriginatorHost + ":" + e.OriginatorPort);
                };
                port2.Start();


                Thread.Sleep(1000 * 60 * 10);

                //System.Threading.Tasks.Parallel.For(0, 100,
                //    (counter) =>
                //    {
                //        var start = DateTime.Now;
                //        var req = HttpWebRequest.Create("http://localhost:8084");
                //        using (var response = req.GetResponse())
                //        {

                //            var data = ReadStream(response.GetResponseStream());
                //            var end = DateTime.Now;

                //            Debug.WriteLine(string.Format("Request# {2}: Lenght: {0} Time: {1}", data.Length, (end - start), counter));
                //        }
                //    }
                //);
            }



            using (var client = new SshClient(connectionInfo))
            {
                client.Connect();
                var cmd = client.CreateCommand("cat test.txt"); // Perform long running task
                cmd.Execute();
                //var asynch = cmd.BeginExecute(null, null);
                //while (!asynch.IsCompleted)
                //{
                //    Console.WriteLine("Waiting for command to complete...");
                //    Thread.Sleep(2000);
                //}
                //cmd.EndExecute(asynch);
                client.Disconnect();
            }


            //var connectionInfo = new KeyboardInteractiveConnectionInfo("152.54.8.155", 2222, "oleg");
            //var connectionInfo = new PrivateKeyConnectionInfo("oleg-centos.edc.renci.org", 22, "tester", new PrivateKeyFile(File.OpenRead(@"H:\My Documents\SSHKeys\oleg-centos.edc.renci.org\rsa_pass_key.txt"), "tester"));
            //connectionInfo.Timeout = TimeSpan.FromSeconds(30);
            //connectionInfo.AuthenticationBanner += delegate(object sender, AuthenticationBannerEventArgs e)
            //{
            //    Console.WriteLine(e.BannerMessage);
            //};

            //    connectionInfo.AuthenticationPrompt += delegate(object sender, AuthenticationPromptEventArgs e)
            //{
            //    foreach (var item in e.Prompts)
            //    {
            //        Console.WriteLine(item.Request);
            //        item.Response = "Maya3kap$";
            //    }
            //};

            //var connectionInfo = new PasswordConnectionInfo("oleg-centos.edc.renci.org", "tester", "tester");
            //connectionInfo.Encryptions.Clear();
            //connectionInfo.Encryptions.Add("blowfish-cbc", typeof(CipherBlowFish));
            using (var cc = new SshClient(connectionInfo))
            {
                cc.KeepAliveInterval = TimeSpan.FromSeconds(5);
                cc.Connect();

                Thread.Sleep(1000 * 30);
                Debug.WriteLine("finished");
            }

            Thread.Sleep(1000 * 30);
            //TestMultipleSftpUploadDownload();


            //using (var sftp = new SftpClient("152.54.8.155", 22, "oleg", "Maya3kap$"))
            //{
            //    sftp.Connect();
            //    var list = sftp.ListDirectory("/home/oleg");
            //    foreach (var item in list)
            //    {
            //        Debug.WriteLine(item);
            //    }
            //    sftp.Disconnect();
            //}

            //var ddd = opensslkey.DecodeOpenSSLPrivateKey(File.ReadAllText(@"H:\My Documents\SSHKeys\openssh.example.txt"));

            //using (var client = new SshClient("oleg-centos.edc.renci.org", "tester", new PrivateKeyFile(File.OpenRead(@"H:\My Documents\SSHKeys\openssh.example.txt"), "tester")))
            //var keys = new PrivateKeyFile[] { 
            //    new PrivateKeyFile(File.OpenRead(@"H:\My Documents\SSHKeys\openssh.example.txt"), "tester"),
            //    //new PrivateKeyFile(File.OpenReasd(@"H:\My Documents\SSHKeys\oleg-centos.edc.renci.org\rsa_pass_key.txt"), "tester1"),
            //    new PrivateKeyFile(File.OpenRead(@"H:\My Documents\SSHKeys\oleg-centos.edc.renci.org\rsa_pass_key.txt"), "tester"),
            //};

            //using (var client = new SshClient("oleg-centos.edc.renci.org", "tester", new PrivateKeyFile(File.OpenRead(@"H:\My Documents\SSHKeys\oleg-centos.edc.renci.org\rsa_pass_key.txt"), "tester")))
            //using (var client = new SshClient("oleg-centos.edc.renci.org", "tester", new PrivateKeyFile(File.OpenRead(@"H:\My Documents\SSHKeys\oleg-centos.edc.renci.org\dsa_pass_key.txt"), "tester")))
            using (var client = new SshClient("152.54.8.155", 2222, "oleg", "Maya3kap$"))
            //using (var client = new SshClient("oleg-centos.edc.renci.org", "tester", keys))
            {
                //client.Authenticating += delegate(object sender, Renci.SshClient.Common.AuthenticationEventArgs e)
                //{
                //    var promptEventrgs = e as AuthenticationPromptEventArgs;
                //    if (promptEventrgs != null)
                //    {
                //        foreach (var item in promptEventrgs.Prompts)
                //        {
                //            Debug.WriteLine(item.Request);
                //            item.Response = "Maya3kap$";
                //        }
                //    }
                //};

                client.Connect();
                var cmd = client.RunCommand("sleep 2s;date");
                Debug.WriteLine(cmd.Result);
                cmd.Execute();
                Debug.WriteLine(cmd.Result);
                cmd.Execute();
                Debug.WriteLine(cmd.Result);
                cmd.Execute();
                Debug.WriteLine(cmd.Result);
                client.Disconnect();
            }

            //var ch = new char[256];
            //for (int i = 0; i < 256; i++)
            //{
            //    ch[i] = (char)i;
            //}
            //var bb = (from c in ch select (byte)c).ToArray();

            //TestMultipleSftpUploadDownload();

            var s = new SshClient("152.54.8.155", 2222, "oleg", "Maya3kap$");
            //var sftp = s.CreateSftp();

            //var async = sftp.BeginListDirectory("/", null, null);
            //var async1 = sftp.BeginListDirectory("/home", null, null);
            //async1.AsyncWaitHandle.WaitOne();
            //async.AsyncWaitHandle.WaitOne();
            //var result = sftp.EndListDirectory(async);
            //var result1 = sftp.EndListDirectory(async1);
            //Thread.Sleep(1000 * 60 * 10);


            // linux/xterm/vt100/vt220/vt320/vt440

            //var input = Console.OpenStandardInput();
            //var input = new MemoryStream(Encoding.ASCII.GetBytes("exit\r\n"));
            //var shell = s.CreateShell(input, Console.Out, Console.Out, "xterm", 80, 24, 800, 600, "");
            //shell.Stopped += delegate(object sender, EventArgs e)
            //{
            //    Console.WriteLine("\nDisconnected...");
            //};
            //shell.Start();
            //Thread.Sleep(1000 * 1000);
            //shell.Stop();

            //var output = new MemoryStream();
            //var output1 = new MemoryStream();
            //shell.Connect(output, output1);
            //Thread.Sleep(1000 * 10);
            //shell.Send("ls -l\r\n");
            //Thread.Sleep(1000 * 10);
            //var result = Encoding.ASCII.GetString(output.ToArray());
            //var port1 = s.AddForwardedPort<ForwardedPortRemote>(0, "localhost", 8080);
            //port1.Start();

            //var cmd = s.CreateCommand(string.Format("echo aaaaaa;sleep 60s;exit 16"));
            ////var cmd = s.CreateCommand(string.Format(";"));
            ////cmd.CommandTimeout = 1000;
            ////var rr = cmd.Execute();
            //var ar = cmd.BeginExecute(null, null);
            //Thread.Sleep(1000 * 3);
            //s.Disconnect();

            //var aaa = cmd.EndExecute(ar);


            System.Threading.Tasks.Parallel.For(0, 10000,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 20,
                },
                (counter) =>
                {
                    Debug.WriteLine(string.Format("Execute thread command. #{0}", counter));
                    var guid = Guid.NewGuid().ToString();
                    var c = s.RunCommand(string.Format("sleep 0s;echo {0}", guid));
                    //var result = s.Shell.Execute(string.Format("echo {0} ; sleep 15s", guid));
                    Debug.WriteLine(string.Format("{2}: {0}: {1}", counter, c.Result.Contains(guid), DateTime.Now));
                    if (!c.Result.Contains(guid))
                    {
                        throw new Exception("Not valid guid.");
                    }
                }
            );

            s.Disconnect();
            return;


            //var client = new SshClient("152.54.8.155", 2222, "oleg", "Maya1kap@");
            //client.Connect();
            //try
            //{

            //    var l = client.Shell.Execute(";");
            //}
            //catch (SshException exp)
            //{
            //    //                throw;
            //}

            //client.Disconnect();
            //client.Connect();
            //var r = client.Shell.Execute("echo 234");
            //client.Disconnect();
            //var port1 = client.AddForwardedPort<ForwardedPortLocal>(8083, "www.cnn.com", 80);
            //port1.Exception += port1_Exception;
            //port1.Start();
            //var port2 = client.AddForwardedPort<ForwardedPortLocal>(8084, "www.renci.org", 80);
            //port2.Exception += port1_Exception;
            //port2.Start();
            //Thread.Sleep(1000 * 5);
            //port1.Stop();
            //Thread.Sleep(1000 * 20);
            //var port = client.AddForwardedPort<ForwardedPortRemote>(8081, "www.renci.org", 80);
            //var port = client.AddForwardedPort<ForwardedPortRemote>(8081, "www.cnn.com", 80);
            //port.Start();
            //var shell = new Shell("152.54.8.155", 2222, "oleg", "Maya1kap@");
            //shell.Connect();
            //client.Shell.Execute("ls -l");
            //shell.Disconnect();


            Thread.Sleep(1000 * 60 * 60);

            System.Threading.Tasks.Parallel.For(0, 100,
                new ParallelOptions
                    {
                        MaxDegreeOfParallelism = 20,
                    },
                (counter) =>
                //            for (int i = 0; i < 10; i++)
                {
                    try
                    {

                        var start = DateTime.Now;
                        //var req = HttpWebRequest.Create("http://localhost:8081/index.html");
                        var req = HttpWebRequest.Create("http://localhost:8081/sample/test.html");
                        using (var response = req.GetResponse())
                        {

                            var data = ReadFully(response.GetResponseStream());
                            var end = DateTime.Now;

                            Debug.WriteLine(string.Format("{2}: Lenght: {0} Time: {1}", data.Length, (end - start), counter));
                        }
                    }
                    catch (Exception exp)
                    {
                        //Debug.WriteLine(exp);
                        //throw;
                    }
                }
            );

            //s.Start(21, "localhost", 21);
            Thread.Sleep(1000 * 60 * 60);
            //s.Disconnect();
        }
/*
        private static void TestCrypto()
        {
            Random r = new Random();


            var keySize = 128;
            var blockSize = 128;
            var mode = System.Security.Cryptography.CipherMode.CBC;

            var key = new byte[keySize / 8];
            var iv = new byte[blockSize / 8];
            var input = new byte[16 * 5];

            r.NextBytes(key);
            r.NextBytes(iv);
            r.NextBytes(input);

            //iv = new byte[] { 0x3a, 0x3f, 0x51, 0xb1, 0xba, 0xc7, 0x60, 0x70, 0x52, 0xb4, 0x8d, 0x21, 0xac, 0x81, 0x27, 0xbb };
            //key = new byte[] { 0xfa, 0x49, 0x9d, 0xa5, 0x59, 0xc3, 0xcc, 0xa9, 0xc3, 0x0b, 0xcd, 0xba, 0x03, 0xbc, 0x97, 0xfc };
            //key = new byte[] { 0xd8, 0xab, 0x36, 0x37, 0x10, 0x20, 0xec, 0x98, 0x90, 0x56, 0xc1, 0x9e, 0xbe, 0x6c, 0x32, 0xc5, 0x77, 0x73, 0x74, 0xee, 0xb5, 0x43, 0xd4, 0x18 };
            //key = new byte[] { 0x81, 0xb3, 0xd3, 0x28, 0x20, 0x0b, 0x27, 0x47, 0xdc, 0x46, 0x6e, 0xee, 0xbb, 0x48, 0xac, 0xc7, 0xee, 0x2e, 0x45, 0xec, 0xb8, 0xd6, 0x7e, 0xfb, 0x03, 0xc9, 0x9b, 0x74, 0x7b, 0x17, 0xf7, 0x0a }; 
            input = new byte[] { 0xcc, 0xa8, 0x81, 0xf2, 0x94, 0xef, 0xcc, 0xfc, 0xc3, 0x49, 0x3c, 0xe5, 0xa3, 0x33, 0x35, 0x34, 0xd7, 0x9a, 0x60, 0xd8, 0x67, 0x5d, 0x51, 0x32, 0xe9, 0x2d, 0x53, 0xa5, 0x35, 0xaa, 0xfc, 0x42, 0xe3, 0x11, 0xed, 0x5b, 0x3a, 0xa4, 0x68, 0x4a, 0x73, 0x55, 0xed, 0x70, 0x8b, 0x7a, 0xcc, 0x1f, 0x7e, 0x58, 0x4a, 0x12, 0x20, 0x4f, 0x53, 0x6f, 0x56, 0x4f, 0x08, 0x66, 0x9b, 0x26, 0xc6, 0x18, 0x4d, 0xfa, 0x01, 0x92, 0x45, 0xda, 0xfa, 0x32, 0x32, 0x16, 0x65, 0x12, 0x70, 0xe0, 0xa6, 0x6f };

            //var key = new byte[] { 0xfa, 0x49, 0x9d, 0xa5, 0x59, 0xc3, 0xcc, 0xa9, 0xc3, 0x0b, 0xcd, 0xba, 0x03, 0xbc, 0x97, 0xfc };
            //var iv = new byte[] { 0x3a, 0x3f, 0x51, 0xb1, 0xba, 0xc7, 0x60, 0x70, 0x52, 0xb4, 0x8d, 0x21, 0xac, 0x81, 0x27, 0xbb };
            //var input = new byte[] { 0xcc, 0xa8, 0x81, 0xf2, 0x94, 0xef, 0xcc, 0xfc, 0xc3, 0x49, 0x3c, 0xe5, 0xa3, 0x33, 0x35, 0x34, 0xd7, 0x9a, 0x60, 0xd8, 0x67, 0x5d, 0x51, 0x32, 0xe9, 0x2d, 0x53, 0xa5, 0x35, 0xaa, 0xfc, 0x42, 0xe3, 0x11, 0xed, 0x5b, 0x3a, 0xa4, 0x68, 0x4a, 0x73, 0x55, 0xed, 0x70, 0x8b, 0x7a, 0xcc, 0x1f, 0x7e, 0x58, 0x4a, 0x12, 0x20, 0x4f, 0x53, 0x6f, 0x56, 0x4f, 0x08, 0x66, 0x9b, 0x26, 0xc6, 0x18, 0x4d, 0xfa, 0x01, 0x92, 0x45, 0xda, 0xfa, 0x32, 0x32, 0x16, 0x65, 0x12, 0x70, 0xe0, 0xa6, 0x6f };

            key.DebugPrint();
            //iv.DebugPrint();
            //input.DebugPrint();

            var output = new byte[input.Length];
            var output1 = new byte[input.Length];

            var doutput = new byte[input.Length];
            var doutput1 = new byte[input.Length];

            var a = new RijndaelManaged();
            //var a = new System.Security.Cryptography.DESCryptoServiceProvider();
            a.KeySize = keySize;
            a.Mode = mode;
            a.Padding = System.Security.Cryptography.PaddingMode.None;

            var enc = a.CreateEncryptor(key, iv);
            Stopwatch watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < 100000; i++)
            {
                enc.TransformBlock(input, 0, input.Length, output, 0);
            }
            watch.Stop();
            Debug.WriteLine("msec: {0}", watch.ElapsedMilliseconds);

            //output.DebugPrint();

            var aes = new Renci.SshClient.Security.Cryptography.Aes(keySize);
            //var aes = new Renci.SshClient.Security.Cryptography.Des(keySize);
            aes.Mode = mode;
            //aes.Mode = (CipherMode)CipherModeEx.CTR;
            var enc1 = aes.CreateEncryptor(key, iv);

            watch.Start();
            for (int i = 0; i < 100000; i++)
            {
                enc1.TransformBlock(input, 0, input.Length, output1, 0);
            }
            watch.Stop();
            Debug.WriteLine("msec: {0}", watch.ElapsedMilliseconds);

            return;
            var enc2 = new BufferedBlockCipher(new Org.BouncyCastle.Crypto.Modes.CbcBlockCipher(new AesFastEngine()));
            //var enc2 = new BufferedBlockCipher(new Org.BouncyCastle.Crypto.Modes.SicBlockCipher(new AesFastEngine()));
            enc2.Init(true, new ParametersWithIV(new KeyParameter(key), iv));
            var output2 = enc2.ProcessBytes(output1);


            if (output.IsEqualTo(output1))
            {
                Debug.WriteLine("Encode Correct");
            }

            if (output2.IsEqualTo(output1))
            {
                Debug.WriteLine("Encode Correct");
            }

            var dec = a.CreateDecryptor(key, iv);
            dec.TransformBlock(output, 0, output.Length, doutput, 0);

            var dec1 = aes.CreateDecryptor(key, iv);
            dec1.TransformBlock(output1, 0, output1.Length, doutput1, 0);

            //var aaa = new BufferedBlockCipher(new Org.BouncyCastle.Crypto.Modes.CfbBlockCipher(new AesFastEngine(), keySize));
            //var aaa = new BufferedBlockCipher(new Org.BouncyCastle.Crypto.Modes.CbcBlockCipher(new AesFastEngine()));
            var aaa = new BufferedBlockCipher(new Org.BouncyCastle.Crypto.Modes.SicBlockCipher(new AesFastEngine()));
            aaa.Init(false, new ParametersWithIV(new KeyParameter(key), iv));
            var doutput2 = aaa.ProcessBytes(output1);

            if (doutput.IsEqualTo(doutput1))
            {
                Debug.WriteLine("Decode correct");
            }

            if (doutput.IsEqualTo(doutput2))
            {
                Debug.WriteLine("Decode Org correct");
            }

            return;
        }

        private static void TestBlowfish()
        {
            var key = new byte[] { 0xe0, 0xb1, 0xb3, 0xbc, 0xa7, 0xe2, 0x2b, 0xca, 0xf0, 0xc1, 0xc7, 0xaa, 0x93, 0x50, 0xb7, 0x72 };
            var iv = new byte[] { 0x0b, 0x7c, 0xf1, 0x33, 0xe0, 0x4a, 0x64, 0x30, 0x37, 0x08, 0x83, 0xd3, 0x56, 0x04, 0x12, 0xd9, 0x88, 0xd0, 0x89, 0xf2, 0xe9, 0xd9, 0x25, 0xea, 0x81, 0x64, 0x5b, 0x03, 0xf2, 0xd3, 0xd5, 0x11 };
            var input = new byte[] { 0x00, 0x00, 0x00, 0x1c, 0x0a, 0x05, 0x00, 0x00, 0x00, 0x0c, 0x73, 0x73, 0x68, 0x2d, 0x75, 0x73, 0x65, 0x72, 0x61, 0x75, 0x74, 0x68, 0x4c, 0x78, 0x42, 0xbb, 0x1a, 0x87, 0x5e, 0xba, 0xfc, 0x9e };
            var output = new byte[input.Length];
            var correct_output = new byte[] { 0x42, 0x36, 0xf4, 0xe9, 0xfd, 0x47, 0x97, 0x43, 0x11, 0x0a, 0x52, 0xf6, 0x1e, 0x7b, 0x78, 0xc0, 0x34, 0x73, 0x58, 0xce, 0x0c, 0xd9, 0x20, 0x97, 0x76, 0x03, 0xb0, 0x2d, 0x4b, 0xce, 0x33, 0x8f };

            var alg = new Renci.SshClient.Security.Cryptography.Blowfish();
            alg.Mode = CipherMode.CBC;
            var enc = alg.CreateEncryptor(key, iv);

            enc.TransformBlock(input, 0, input.Length, output, 0);

        }
*/
        private static void ListCurrentDirectory(SftpClient sftp, string path = ".")
        {
            Console.WriteLine("Directory:" + sftp.WorkingDirectory);
            var files = sftp.ListDirectory(path).ToList();
            foreach (var file in files)
            {
                Console.WriteLine(file.FullName);
            }
        }

        public static void CreateTestFile(string path, int size)
        {
            //  Create test files
            for (int j = 0; j < 5; j++)
            {
                using (var testFile = File.Create(path))
                {
                    var random = new Random();
                    for (int i = 0; i < size; i++)
                    {
                        var buffer = new byte[1024];
                        random.NextBytes(buffer);
                        testFile.Write(buffer, 0, buffer.Length);
                        testFile.Flush();
                    }
                }
            }
        }

        private static void TestMultipleSftpUploadDownload()
        {

            //  Create test files
            //for (int j = 0; j < 5; j++)
            //{
            //    using (var testFile = File.Create(string.Format("C:\\Test\\TestFile{0}.bin", j)))
            //    {

            //        var random = new Random();
            //        for (int i = 0; i < 1024 * 100; i++)
            //        {
            //            var buffer = new byte[1024];
            //            random.NextBytes(buffer);
            //            testFile.Write(buffer, 0, buffer.Length);
            //        }
            //    }
            //}

            //  Calculate check sum

            for (int i = 0; i < 5; i++)
            {
                var name = string.Format("C:\\Test\\TestFile{0}.bin", i);
                var md5 = GetMD5HashFromFile(name);

                Console.WriteLine("name: {0}, checksum: {1}", name, md5);
            }

            var sftp = new SftpClient("152.54.8.155", 22, "oleg", "Maya3kap$");
            sftp.Connect();

            var mem = File.OpenRead("C:\\Test\\TestFile0.bin");
            var mem1 = File.OpenRead("C:\\Test\\TestFile1.bin");
            var mem2 = File.OpenRead("C:\\Test\\TestFile2.bin");
            var mem3 = File.OpenRead("C:\\Test\\TestFile3.bin");
            var mem4 = File.OpenRead("C:\\Test\\TestFile4.bin");
            var asynch = sftp.BeginUploadFile(mem, "/home/oleg/test1.txt", null, null);
            var asynch1 = sftp.BeginUploadFile(mem1, "/home/oleg/test2.txt", null, null);
            var asynch2 = sftp.BeginUploadFile(mem2, "/home/oleg/test3.txt", null, null);
            var asynch3 = sftp.BeginUploadFile(mem3, "/home/oleg/test4.txt", null, null);
            var asynch4 = sftp.BeginUploadFile(mem4, "/home/oleg/test5.txt", null, null);

            var sftpASynch = asynch as SftpAsyncResult;
            var sftpASynch1 = asynch1 as SftpAsyncResult;
            var sftpASynch2 = asynch2 as SftpAsyncResult;
            var sftpASynch3 = asynch3 as SftpAsyncResult;
            var sftpASynch4 = asynch4 as SftpAsyncResult;
            while (!sftpASynch.IsCompleted && !sftpASynch1.IsCompleted && !sftpASynch2.IsCompleted && !sftpASynch3.IsCompleted && !sftpASynch4.IsCompleted)
            {
                Console.Write(string.Format("\rUploaded {0:#########} KB Uploaded {1:#########} KB Uploaded {2:#########} KB Uploaded {3:#########} KB Uploaded {4:#########} KB", (sftpASynch.UploadedBytes / 1024), (sftpASynch1.UploadedBytes / 1024), (sftpASynch2.UploadedBytes / 1024), (sftpASynch3.UploadedBytes / 1024), (sftpASynch4.UploadedBytes / 1024)));
                Thread.Sleep(100);
            }

            sftp.EndUploadFile(asynch);
            sftp.EndUploadFile(asynch1);
            sftp.EndUploadFile(asynch2);
            sftp.EndUploadFile(asynch3);
            sftp.EndUploadFile(asynch4);

            mem = File.Create("C:\\Test\\TestFile0_out.bin");
            mem1 = File.Create("C:\\Test\\TestFile1_out.bin");
            mem2 = File.Create("C:\\Test\\TestFile2_out.bin");
            mem3 = File.Create("C:\\Test\\TestFile3_out.bin");
            mem4 = File.Create("C:\\Test\\TestFile4_out.bin");

            asynch = sftp.BeginDownloadFile("/home/oleg/test1.txt", mem, null, null);
            asynch1 = sftp.BeginDownloadFile("/home/oleg/test2.txt", mem1, null, null);
            asynch2 = sftp.BeginDownloadFile("/home/oleg/test3.txt", mem2, null, null);
            asynch3 = sftp.BeginDownloadFile("/home/oleg/test4.txt", mem3, null, null);
            asynch4 = sftp.BeginDownloadFile("/home/oleg/test5.txt", mem4, null, null);

            sftpASynch = asynch as SftpAsyncResult;
            sftpASynch1 = asynch1 as SftpAsyncResult;
            sftpASynch2 = asynch2 as SftpAsyncResult;
            sftpASynch3 = asynch3 as SftpAsyncResult;
            sftpASynch4 = asynch4 as SftpAsyncResult;
            Console.WriteLine("Downloading");
            while (!sftpASynch.IsCompleted && !sftpASynch1.IsCompleted && !sftpASynch2.IsCompleted && !sftpASynch3.IsCompleted && !sftpASynch4.IsCompleted)
            {
                Console.Write(string.Format("\rDownloaded {0:#########} MB Downloaded {1:#########} MB Downloaded {2:#########} MB Downloaded {3:#########} MB Downloaded {4:#########} MB", (sftpASynch.DownloadedBytes / 1024), (sftpASynch1.DownloadedBytes / 1024), (sftpASynch2.DownloadedBytes / 1024), (sftpASynch3.DownloadedBytes / 1024), (sftpASynch4.DownloadedBytes / 1024)));
                Thread.Sleep(100);
            }

            sftp.EndDownloadFile(asynch);
            sftp.EndDownloadFile(asynch1);
            sftp.EndDownloadFile(asynch2);
            sftp.EndDownloadFile(asynch3);
            sftp.EndDownloadFile(asynch4);

            mem.Close();
            mem1.Close();
            mem2.Close();
            mem3.Close();
            mem4.Close();

            Console.WriteLine("Checksums");
            for (int i = 0; i < 5; i++)
            {
                var name = string.Format("C:\\Test\\TestFile{0}_out.bin", i);
                var md5 = GetMD5HashFromFile(name);

                Console.WriteLine("name: {0}, checksum: {1}", name, md5);
            }

            sftp.Disconnect();

        }

        public static byte[] ReadFully(Stream stream)
        {
            byte[] buffer = new byte[1024];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        ms.Write(buffer, 0, read);
                    else
                        return ms.ToArray();
                }
            }
        }

        protected static string GetMD5HashFromFile(string fileName)
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }

        private static byte[] ReadStream(Stream stream)
        {
            byte[] buffer = new byte[1024];
            using (MemoryStream ms = new MemoryStream())
            {
                while (true)
                {
                    int read = stream.Read(buffer, 0, buffer.Length);
                    if (read > 0)
                        ms.Write(buffer, 0, read);
                    else
                        return ms.ToArray();
                }
            }
        }

    }
}
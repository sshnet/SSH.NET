using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Renci.SshClient.Test
{
    class Program
    {
        static void Main(string[] args)
        {

            //var compressed1950 = new byte[] { 0x78, 0x9c, 0x62, 0x64, 0x60, 0x60, 0x60, 0x02, 0x62, 0xf1, 0x80, 0xc4, 0xe4, 0xec, 0xd4, 0x12, 0x85, 0xcc, 0xbc, 0x92, 0xd4, 0xf4, 0xa2, 0xcc, 0x92, 0x4a, 0x85, 0xd4, 0xa2, 0xa2, 0xfc, 0x22, 0x3d, 0xa0, 0x14, 0x03, 0x40 };
            //var compressed1951 = new byte[] { 0x62, 0x64, 0x60, 0x60, 0x60, 0x02, 0x62, 0xf1, 0x80, 0xc4, 0xe4, 0xec, 0xd4, 0x12, 0x85, 0xcc, 0xbc, 0x92, 0xd4, 0xf4, 0xa2, 0xcc, 0x92, 0x4a, 0x85, 0xd4, 0xa2, 0xa2, 0xfc, 0x22, 0x3d, 0xa0, 0x14, 0x03, 0x40 };
            //var uncompressed = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x17, 0x50, 0x61, 0x63, 0x6b, 0x65, 0x74, 0x20, 0x69, 0x6e, 0x74, 0x65, 0x67, 0x72, 0x69, 0x74, 0x79, 0x20, 0x65, 0x72, 0x72, 0x6f, 0x72, 0x2e, 0x00, 0x00, 0x00, 0x00 };
            //var uncompressed = "ABCDEFG".GetSshBytes().ToArray();

            //var data = new byte[] { 87, 105, 107, 105, 112, 101, 100, 105, 97 };
            //var aa = CompressionZlib.Adler32(uncompressed);
            ////8f da 08 ea
            //var a1 = (byte)((aa & 0xFF000000) >> 24);
            //var a2 = (byte)((aa & 0x00FF0000) >> 16);
            //var a3 = (byte)((aa & 0x0000FF00) >> 8);
            //var a4 = (byte)((aa & 0x000000FF) >> 0);
            //Console.WriteLine("{0:x2} {1:x2} {2:x2} {3:x2} ", a1, a2, a3, a4);

            //byte[] result1 = null;
            //using (var output = new MemoryStream())
            //{
            //    using (var input = new MemoryStream(uncompressed))
            //    using (var compress = new DeflateStream(output, CompressionMode.Compress))
            //    {
            //        compress.FlushMode = FlushType.Partial;
            //        // Copy the source file into 
            //        // the compression stream.
            //        input.CopyTo(compress);

            //        result1 = output.ToArray();
            //    }
            //}
            //result1.DebugPrint();

            //byte[] result11 = null;
            //using (var output = new MemoryStream())
            //{
            //    using (var input = new MemoryStream(uncompressed))
            //    using (var compress = new System.IO.Compression.DeflateStream(output, System.IO.Compression.CompressionMode.Compress))
            //    {
            //        // Copy the source file into 
            //        // the compression stream.
            //        input.CopyTo(compress);

            //        result11 = output.ToArray();
            //    }
            //}
            //result11.DebugPrint();

            //byte[] result2 = null;
            //using (var output = new MemoryStream())
            //{
            //    using (var decompress = new DeflateStream(new MemoryStream(result1), CompressionMode.Decompress))
            //    {
            //        // Copy the decompression stream 
            //        // into the output file.
            //        decompress.CopyTo(output);
            //        result2 = output.ToArray();
            //    }
            //}

            //CompressionZlib c = new CompressionZlib();

            //var d = new byte[] { 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65, 65 };
            //var rr = c.Deflate(d);
            //var b = c.Inflate(rr);

            //var baseFile = @"D:\small.txt";
            //var inFile = new FileInfo(string.Format("{0}", baseFile));
            //var outFile = new FileInfo(string.Format("{0}.gz", baseFile));
            //Compress(inFile.OpenRead(), outFile.Create());
            //var f = File.Create(string.Format("{0}.txt", baseFile));
            //Decompress(outFile.OpenRead(), f);
            //f.Close();

            //using (var file = File.OpenWrite("D:\\zerobunary.dat"))
            //{
            //    for (int j = 0; j < 10; j++)
            //    {
            //        for (int i = 0; i < 1024 * 1024 * 1024; i++)
            //        {
            //            file.WriteByte(0);
            //        }
            //    }
            //}

            //TestSftp();
            //TestSftpDownload();


            //var shell = CreateShell();
            //shell.Connect();
            //var res = shell.Execute("ls -l");
            //shell.Disconnect();
            //Console.WriteLine(DateTime.Now + ":" + res.Length);
            //res = shell.Execute("ls -l");
            //Console.WriteLine(DateTime.Now + ":" + res.Length);
            //res = shell.Execute("ls -l");
            //Console.WriteLine(DateTime.Now + ":" + res.Length);
            //shell.Disconnect();

            //for (int i = 0; i < 2; i++)
            //{
            //    TestExec(i);
            //}


            //  Parallel testing
            var startTick = DateTime.Now.Ticks;
            TestParallelConnections();
            //TestParallelExec();
            var endTick = DateTime.Now.Ticks;
            //TestParallelAynchExec();

            //s.Disconnect();
            Console.WriteLine("Finished in " + ((double)(endTick - startTick) / 10000000));
            Console.ReadKey();
        }

        private static volatile int _counter = 0;

        private static volatile int _errors = 0;

        private static volatile int _connectionCount;

        private static void TestParallelExec()
        {
            var shell = CreateShell();
            int _threadsCount = 0;

            System.Threading.Tasks.Parallel.For(0, 100,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 15
                },
                () =>
                {
                    _threadsCount++;
                    return new object();
                },
                (int counter, ParallelLoopState pls, object conn) =>
                //(counter) =>
                //for (int counter = 0; counter < 100; counter++)
                {
                    try
                    {
                        var result = EchoTest(shell, counter);
                        if (result == false)
                        {
                            _errors++;
                        }
                        Console.WriteLine("{5:000}: {4}: Count: {3:000}, Thread: {0:000}, i: {1:0000}, Result: {2:000000}", Thread.CurrentThread.ManagedThreadId, counter, result, _counter++, DateTime.Now, _threadsCount);
                    }
                    catch (Exception exp)
                    {
                        Console.WriteLine("{5:000}: {4}: Count: {3:000}, Thread: {0:000}, i: {1:0000}, Error: {2:000000}", Thread.CurrentThread.ManagedThreadId, counter, exp.Message, _counter++, DateTime.Now, _threadsCount);
                        _errors++;
                    }
                    return null;
                },
                (object conn) =>
                {
                    _threadsCount--;
                }
            );

            shell.Disconnect();

            Console.WriteLine("Parallel execution tested");
            //Console.ReadKey();
        }

        private static void TestParallelAynchExec()
        {
            var shell = CreateShell();

            System.Threading.Tasks.Parallel.For(0, 100000,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 10
                },
            (counter) =>
            {
                try
                {
                    MemoryStream result = new MemoryStream();
                    //var asyncResult = shell.BeginExecute("cat bigtestfile.txt", result, null, null);
                    var asyncResult = shell.BeginExecute("ls -l", result, null, null);

                    asyncResult.AsyncWaitHandle.WaitOne();

                    shell.EndExecute(asyncResult);

                    var r = Encoding.ASCII.GetString(result.ToArray());

                    Console.WriteLine("{4}: Count: {3:000}, Thread: {0:000}, i: {1:000}, Bytes: {2:000000}", Thread.CurrentThread.ManagedThreadId, counter, r.Length, _counter++, DateTime.Now);
                }
                catch (Exception exp)
                {
                    Console.WriteLine("{4}: Count: {3:000}, Thread: {0:000}, i: {1:000}, Error: {2:000000}", Thread.CurrentThread.ManagedThreadId, counter, exp.Message, _counter++, DateTime.Now);
                    _errors++;
                }
            }
            );

            Console.WriteLine("Parallel execution tested");
            //Console.ReadKey();
        }

        private static void TestParallelConnections()
        {
            System.Threading.Tasks.Parallel.For(0, 10000,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = 10
                },
                () =>
                {
                    var conn = CreateShell();
                    conn.Connect();
                    _connectionCount++;
                    return conn;
                },
                (int counter, ParallelLoopState pls, Shell conn) =>
                //(int counter) =>
                {

                    //var conn = new Shell("152.54.9.105", 22, "oleg", "Maya1kap@");
                    //conn.Connect();
                    //_connectionCount++;

                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0}", DateTime.Now);
                    sb.AppendFormat("Conn: {0:000} ", _connectionCount);
                    sb.AppendFormat("Thread: {0:000} ", Thread.CurrentThread.ManagedThreadId);
                    sb.AppendFormat("Count: {0:0000} ", _counter++);
                    sb.AppendFormat("i: {0:0000} ", counter);
                    try
                    {

                        var result = EchoTest(conn, counter);
                        if (result == false)
                        {
                            _errors++;
                        }

                        //var r = conn.Execute("cat bigtestfile.txt");
                        //sb.AppendFormat("Bytes: {0:000000} ", r.Length);
                        sb.AppendFormat("Bytes: {0:000000} ", result);
                    }
                    catch (Exception exp)
                    {
                        sb.AppendFormat("Error: {0}", exp.Message);
                        _errors++;
                    }
                    Console.WriteLine(sb.ToString());

                    //conn.Disconnect();
                    //_connectionCount--;

                    return conn;
                },
                (Shell conn) =>
                {
                    conn.Disconnect();
                    _connectionCount--;
                }
                );

            Console.WriteLine("Parallel execution tested");

            //Console.ReadKey();
        }

        private static void TestSftpDownload()
        {
            Sftp sftp = new Sftp("152.54.8.155", 2222, "oleg", "Maya1kap@");
            sftp.DownloadFile("/home/oleg/uploadedfile.txt", @"D:\downloadtext1.txt");
            Console.WriteLine("SFTP tested.");
            Console.ReadKey();
        }

        private static void TestSftp()
        {
            Sftp sftp = new Sftp("152.54.9.105", 2222, "oleg", "Maya1kap@");
            //sftp.UploadFile(@"D:\NoaaGetCapabilities.xml", "/home/oleg/uploadedfile.txt");            
            sftp.UploadFile(@"D:\zerobunary.dat", "/home/oleg/uploadedfile.txt");
            sftp.UploadFile(@"D:\SQL Server Data\BCFS_0.ldf", "/home/oleg/uploadedfile.txt");

            sftp.DownloadFile("/home/oleg/uploadedfile.txt", @"D:\downloadtext1.txt");

            Console.WriteLine("SFTP tested.");
        }

        private static void TestExec(int counter)
        {
            var s = CreateShell();
            s.Connect();
            MemoryStream debugStream = new MemoryStream();

            //var res1 = s.Execute("cat bigtestfile.txt", debugStream);
            //var res2 = Encoding.ASCII.GetString(debugStream.ToArray());
            var result = EchoTest(s, counter);


            Console.WriteLine("{4}: Count: {3:000}, Thread: {0:000}, i: {1:000}, Bytes: {2:000000}", Thread.CurrentThread.ManagedThreadId, counter, result, _counter++, DateTime.Now);
            s.Disconnect();

            //Console.WriteLine("Execute tested.");
            //Console.ReadKey();
        }

        private static bool EchoTest(Shell shell, int counter)
        {
            bool result;
            var length = 1024 * 16;
            var btf = shell.Execute(string.Format("for i in {{1..{0}}}\ndo\necho \"ABCDEFG\"\ndone\n", length / 8));

            result = (btf.Length == length);
            if (!result)
                return result;

            var r = shell.Execute(string.Format("echo {0}", counter));
            int r1;
            if (int.TryParse(r, out r1))
            {
                result = (r1 == counter);
            }
            else
            {
                result = false;
            }

            //shell.Execute("sleep 10");

            return result;
        }

        private static Shell CreateShell()
        {
            return new Shell("152.54.8.155", 22, "oleg", "Maya1kap@");
            //return new Shell("152.54.8.155", 22, "oleg", "Maya1kap@");
            //return new Shell("br0.renci.org", 22, "wrf", new PrivateKeyFile(@"H:\My Documents\SSHKeys\useme.key"));
        }

    }
}

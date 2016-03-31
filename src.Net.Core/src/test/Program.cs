using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //PasswordConnectionInfo conn = new PasswordConnectionInfo("[IP]", 22, "root", "[password]");
            //SshClient sshClient = new SshClient(conn);
            //sshClient.Connect();


            //while (true)
            //{
            //    var command = Console.ReadLine();

            //    var comm = sshClient.CreateCommand(command);
            //    comm.CommandTimeout = TimeSpan.FromSeconds(30);

            //    Console.WriteLine(comm.Execute());
            //}


            PasswordConnectionInfo conn = new PasswordConnectionInfo("[IP]", 22, "root", "[password]");
            SftpClient sftpClient = new SftpClient(conn);
            sftpClient.Connect();

            //using (var output = File.OpenWrite(@"c:\!temp\syslog8"))
            //{
            //    var input =  sftpClient.OpenRead("/opt/test/syslog8");

            //    input.CopyToAsync(output).Wait();
            //}

            var d = sftpClient.ListDirectory("/");
            Console.WriteLine(string.Join(", ", d.Select(s => s.Name)));
        }
    }
}

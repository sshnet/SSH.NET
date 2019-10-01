using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace renci.sftp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return;
            }

            int port = 22;
            string destination = "127.0.0.1";
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "P")
                {
                    if (i + 1 < args.Length)
                    {
                        port = int.Parse(args[i + 1]);
                        i++;
                    }
                }

                destination = args[i];
            }

            Console.Write("user: ");
            string username = Console.ReadLine();
            Console.Write("pwd: ");
            string password = ReadPassword();
            Console.WriteLine("connecting...");
            Renci.SshNet.SftpClient.ChangeDirIsLocal = true;
            using (var client = new Renci.SshNet.SftpClient(destination, port, username, password))
            {
                client.Connect();
                while (true)
                {
                    Console.WriteLine();
                    string current = client.WorkingDirectory;
                    Console.Write(current);
                    Console.Write(">>");Console.Out.Flush();
                    string line = Console.ReadLine();
                    try
                    {
                        if (!Process(client, line.Split(' ')))
                        {
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private static bool Process(Renci.SshNet.SftpClient client, string[] line)
        {
            switch (line[0])
            {
                case "cd":
                    if (line.Length == 1)
                    {
                        Console.WriteLine(client.WorkingDirectory);
                    }
                    else
                    {
                        client.ChangeDirectory(line[1]);
                    }
                    break;
                case "pwd":
                    Console.WriteLine(Environment.CurrentDirectory);
                    break;
                case "rm":
                    client.DeleteFile(line[1]);
                    break;
                case "put":
                    using (var l = File.OpenRead(line[1]))
                    {
                        string dest = (line.Length > 2 ? line[2] : line[1]);
                        using (var f = client.OpenWrite(dest))
                        {
                            byte[] buffer = new byte[1024 * 16];
                            int r;
                            while ((r = l.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                f.Write(buffer, 0, r);
                            }
                            f.Flush();
                        }
                    }
                    break;
                case "cat":
                    Console.WriteLine(client.ReadAllText(line[1]));
                    break;
                case "get":
                    using (var l = client.OpenRead(line[1]))
                    {
                        string dest = (line.Length > 2 ? line[2] : line[1]);
                        using (var f = File.OpenWrite(dest))
                        {
                            byte[] buffer = new byte[1024 * 16];
                            int r;
                            while ((r = l.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                f.Write(buffer, 0, r);
                            }
                            f.Flush();
                        }
                    }
                    break;
                case "dir":
                    {
                        string path = (line.Length > 1 ? line[1] : ".");
                        foreach (var el in Directory.GetFileSystemEntries(path))
                        {
                            Console.WriteLine(el);
                        }
                        break;
                    }
                case "ls":
                    {
                        string path = (line.Length > 1 ? line[1] : ".");

                        foreach (var el in client.ListDirectory(path))
                        {
                            Console.WriteLine(el.FullName);
                        }
                        break;
                    }
                case "exit":
                case "quit":
                    return false;
                default:
                    throw new ArgumentException("unsupported command " + line[0]);
            }

            return true;
        }

        private static string ReadPassword()
        {
            string pass = String.Empty;
            do
            {
                var key = Console.ReadKey(true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    pass = pass.Substring(0, (pass.Length - 1));
                    Console.Write("\b \b");
                }
                else
                {
                    pass = pass + key.KeyChar;
                }
            } while (true);

            return pass;
        }

        private static void Usage()
        {
            Console.WriteLine(@"usage: Renci.sftp [-P port] destination");
        }
    }
}

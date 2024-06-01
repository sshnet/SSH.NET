Think this page is lacking? Help wanted! Click "Edit this page" at the bottom to begin contributing more examples.

Getting Started
=================

### Run a command

Establish an SSH connection and run a command:

```cs
using (var client = new SshClient("sftp.foo.com", "guest", new PrivateKeyFile("path/to/my/key")))
{
    client.Connect();
    SshCommand cmd = client.RunCommand("echo 'Hello World!'");
    Console.WriteLine(cmd.Result); // "Hello World!\n"
}
```

### Upload and list files

SFTP Connection / Exchange 

```cs
using (var client = new SftpClient("sftp.foo.com", "guest", "pwd"))
{
    client.Connect();

    using (FileStream fs = File.OpenRead(@"C:\tmp\test-file.txt"))
    {
        client.UploadFile(fs, "/home/guest/test-file.txt");
    }

    foreach (ISftpFile file in client.ListDirectory("/home/guest/"))
    {
        Console.WriteLine($"{file.FullName} {file.LastWriteTime}");
    }
}
```

### Multi-factor authentication

Establish an SFTP connection using both password and public-key authentication:

```cs
var connectionInfo = new ConnectionInfo("sftp.foo.com",
                                        "guest",
                                        new PasswordAuthenticationMethod("guest", "pwd"),
                                        new PrivateKeyAuthenticationMethod("path/to/my/key"));
using (var client = new SftpClient(connectionInfo))
{
    client.Connect();
}
```

### Verify host identify

Establish an SSH connection using user name and password, and reject the connection if the fingerprint of the server does not match the expected fingerprint:

```cs
string expectedFingerPrint = "LKOy5LvmtEe17S4lyxVXqvs7uPMy+yF79MQpHeCs/Qo";

using (var client = new SshClient("sftp.foo.com", "guest", "pwd"))
{
    client.HostKeyReceived += (sender, e) =>
        {
            e.CanTrust = expectedFingerPrint.Equals(e.FingerPrintSHA256);
        };
    client.Connect();
}
```

### Open a Shell  

```cs
using (var client = new SshClient("sftp.foo.com", "user", "password"))
{
    client.Connect();
    ShellStream shellStream = client.CreateShellStream("ShellName", 80, 24, 800, 600, 1024);
    client.Disconnect();
}
```

### Switch to root with "su - root"

```cs
using (var client = new SshClient("sftp.foo.com", "user", "password"))
{
    client.Connect();
    ShellStream shellStream = client.CreateShellStream("ShellName", 80, 24, 800, 600, 1024);
    // Get logged in and get user prompt
    string prompt = stream.Expect(new Regex(@"[$>]"));
    // Send command and expect password or user prompt
    stream.WriteLine("su - root");
    prompt = stream.Expect(new Regex(@"([$#>:])"));
    // Check to send password
    if (prompt.Contains(":"))
    {
        // Send password
        stream.WriteLine("password");
        prompt = stream.Expect(new Regex(@"[$#>]"));
    }
    client.Disconnect();
}
```


### Stream data to a command

```cs
using (var client = new SshClient("sftp.foo.com", "guest", "pwd"))
{
    client.Connect();

    // Make the server echo back the input file with "cat"
    using (SshCommand command = client.CreateCommand("cat"))
    {
        IAsyncResult asyncResult = command.BeginExecute();

        using (Stream inputStream = command.CreateInputStream())
        {
            inputStream.Write("Hello World!"u8);
        }

        string result = command.EndExecute(asyncResult);

        Console.WriteLine(result); // "Hello World!"
    }
}
```

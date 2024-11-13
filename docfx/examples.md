Think this page is lacking? Help wanted! Click "Edit this page" at the bottom to begin contributing more examples.

Getting Started
=================

### Run a command

```cs
using (var client = new SshClient("sftp.foo.com", "guest", new PrivateKeyFile("path/to/my/key")))
{
    client.Connect();
    using SshCommand cmd = client.RunCommand("echo 'Hello World!'");
    Console.WriteLine(cmd.Result); // "Hello World!\n"
}
```

### Upload and list files using SFTP

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

Establish a connection using both password and public-key authentication:

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

Establish a connection using user name and password, and reject the connection if the fingerprint of the server does not match the expected fingerprint:

```cs
string expectedFingerPrint = "LKOy5LvmtEe17S4lyxVXqvs7uPMy+yF79MQpHeCs/Qo";

using (var client = new SshClient("sftp.foo.com", "guest", "pwd"))
{
    client.HostKeyReceived += (sender, e) =>
    {
        e.CanTrust = e.FingerPrintSHA256 == expectedFingerPrint;
    };
    client.Connect();
}
```

When expecting the server to present a certificate signed by a trusted certificate authority:

```cs
string expectedCAFingerPrint = "tF3DRTUXtYFZ5Yz0SBOrEbixHaCifHmNVK6FtptXZVM";

using (var client = new SshClient("sftp.foo.com", "guest", "pwd"))
{
    client.HostKeyReceived += (sender, e) =>
    {
        e.CanTrust = e.Certificate?.CertificateAuthorityKeyFingerPrint == expectedCAFingerPrint;
    };
    client.Connect();
}
```

### Authenticating with a user certificate

When you have a certificate for your key which is signed by a certificate authority that the server trusts:

```cs
using (var privateKeyFile = new PrivateKeyFile("path/to/my/key", passPhrase: null, "path/to/my/certificate.pub"))
using (var client = new SshClient("sftp.foo.com", "guest", privateKeyFile))
{
    client.Connect();
}
```

### Open a Shell  

```cs
using (var client = new SshClient("sftp.foo.com", "user", "password"))
{
    client.Connect();
    using ShellStream shellStream = client.CreateShellStream("ShellName", 80, 24, 800, 600, 1024);
    client.Disconnect();
}
```

### Switch to root with "su - root"

```cs
using (var client = new SshClient("sftp.foo.com", "user", "password"))
{
    client.Connect();
    using ShellStream shellStream = client.CreateShellStream("ShellName", 80, 24, 800, 600, 1024);
    // Get logged in and get user prompt
    string prompt = shellStream.Expect(new Regex(@"[$>]"));
    // Send command and expect password or user prompt
    shellStream.WriteLine("su - root");
    prompt = shellStream.Expect(new Regex(@"([$#>:])"));
    // Check to send password
    if (prompt.Contains(":"))
    {
        // Send password
        shellStream.WriteLine("password");
        prompt = shellStream.Expect(new Regex(@"[$#>]"));
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
        Task executeTask = command.ExecuteAsync(CancellationToken.None);

        using (Stream inputStream = command.CreateInputStream())
        {
            inputStream.Write("Hello World!"u8);
        }

        await executeTask;

        Console.WriteLine(command.ExitStatus); // 0
        Console.WriteLine(command.Result); // "Hello World!"
    }
}
```

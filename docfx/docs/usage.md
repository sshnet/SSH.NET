# Usage

## Multi-factor authentication

Establish a SFTP connection using both password and public-key authentication:

```cs
var connectionInfo = new ConnectionInfo("sftp.foo.com",
                                        "guest",
                                        new PasswordAuthenticationMethod("guest", "pwd"),
                                        new PrivateKeyAuthenticationMethod("rsa.key"));
using (var client = new SftpClient(connectionInfo))
{
    client.Connect();
}

```

## Verify host identify

Establish a SSH connection using user name and password, and reject the connection if the fingerprint of the server does not match the expected fingerprint:

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

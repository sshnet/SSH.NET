 ![Logo](https://raw.githubusercontent.com/sshnet/SSH.NET/develop/images/logo/png/SS-NET-icon-h50.png) SSH.NET
=======
SSH.NET is a Secure Shell (SSH-2) library for .NET, optimized for parallelism.

[![Version](https://img.shields.io/nuget/vpre/SSH.NET.svg)](https://www.nuget.org/packages/SSH.NET)
[![NuGet download count](https://img.shields.io/nuget/dt/SSH.NET.svg)](https://www.nuget.org/packages/SSH.NET)
![Build status](https://github.com/sshnet/SSH.NET/actions/workflows/build.yml/badge.svg)

## Key Features

* Execution of SSH command using both synchronous and asynchronous methods
* SFTP functionality for both synchronous and asynchronous operations
* SCP functionality
* Remote, dynamic and local port forwarding 
* Interactive shell/terminal implementation
* Authentication via publickey, password and keyboard-interactive methods, including multi-factor
* Connection via SOCKS4, SOCKS5 or HTTP proxy

## How to Use

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

## Main Types

The main types provided by this library are:

* Renci.SshNet.SshClient
* Renci.SshNet.SftpClient
* Renci.SshNet.ScpClient
* Renci.SshNet.PrivateKeyFile
* Renci.SshNet.SshCommand
* Renci.SshNet.ShellStream

## Additional Documentation

* [Further examples](https://sshnet.github.io/SSH.NET/examples.html)
* [Logging](https://sshnet.github.io/SSH.NET/logging.html)
* [API browser](https://sshnet.github.io/SSH.NET/api/Renci.SshNet.html)

## Encryption Methods

**SSH.NET** supports the following encryption methods:
* aes128-ctr
* aes192-ctr
* aes256-ctr
* aes128-gcm<span></span>@openssh.com
* aes256-gcm<span></span>@openssh.com
* chacha20-poly1305<span></span>@openssh.com
* aes128-cbc
* aes192-cbc
* aes256-cbc
* 3des-cbc

## Key Exchange Methods

**SSH.NET** supports the following key exchange methods:
* mlkem768x25519-sha256
* sntrup761x25519-sha512
* sntrup761x25519-sha512<span></span>@openssh.com
* curve25519-sha256
* curve25519-sha256<span></span>@libssh.org
* ecdh-sha2-nistp256
* ecdh-sha2-nistp384
* ecdh-sha2-nistp521
* diffie-hellman-group-exchange-sha256
* diffie-hellman-group-exchange-sha1
* diffie-hellman-group16-sha512
* diffie-hellman-group14-sha256
* diffie-hellman-group14-sha1
* diffie-hellman-group1-sha1

## Public Key Authentication

**SSH.NET** supports the following private key formats:
* RSA in
  * OpenSSL traditional PEM format ("BEGIN RSA PRIVATE KEY")
  * OpenSSL PKCS#8 PEM format ("BEGIN PRIVATE KEY", "BEGIN ENCRYPTED PRIVATE KEY")
  * ssh.com format ("BEGIN SSH2 ENCRYPTED PRIVATE KEY")
  * OpenSSH key format ("BEGIN OPENSSH PRIVATE KEY")
  * PuTTY private key format ("PuTTY-User-Key-File-2", "PuTTY-User-Key-File-3")
* DSA in
  * OpenSSL traditional PEM format ("BEGIN DSA PRIVATE KEY")
  * OpenSSL PKCS#8 PEM format ("BEGIN PRIVATE KEY", "BEGIN ENCRYPTED PRIVATE KEY")
  * ssh.com format ("BEGIN SSH2 ENCRYPTED PRIVATE KEY")
  * PuTTY private key format ("PuTTY-User-Key-File-2", "PuTTY-User-Key-File-3")
* ECDSA 256/384/521 in
  * OpenSSL traditional PEM format ("BEGIN EC PRIVATE KEY")
  * OpenSSL PKCS#8 PEM format ("BEGIN PRIVATE KEY", "BEGIN ENCRYPTED PRIVATE KEY")
  * OpenSSH key format ("BEGIN OPENSSH PRIVATE KEY")
  * PuTTY private key format ("PuTTY-User-Key-File-2", "PuTTY-User-Key-File-3")
* ED25519 in
  * OpenSSL PKCS#8 PEM format ("BEGIN PRIVATE KEY", "BEGIN ENCRYPTED PRIVATE KEY")
  * OpenSSH key format ("BEGIN OPENSSH PRIVATE KEY")
  * PuTTY private key format ("PuTTY-User-Key-File-2", "PuTTY-User-Key-File-3")

Private keys in OpenSSL traditional PEM format can be encrypted using one of the following cipher methods:
* DES-EDE3-CBC
* DES-EDE3-CFB
* AES-128-CBC
* AES-192-CBC
* AES-256-CBC

Private keys in OpenSSL PKCS#8 PEM format can be encrypted using any cipher method BouncyCastle supports.

Private keys in ssh.com format can be encrypted using the following cipher method:
* 3des-cbc

Private keys in OpenSSH key format can be encrypted using one of the following cipher methods:
* 3des-cbc
* aes128-cbc
* aes192-cbc
* aes256-cbc
* aes128-ctr
* aes192-ctr
* aes256-ctr
* aes128-gcm<span></span>@openssh.com
* aes256-gcm<span></span>@openssh.com
* chacha20-poly1305<span></span>@openssh.com

Private keys in PuTTY private key format can be encrypted using the following cipher method:
* aes256-cbc

## Host Key Algorithms

**SSH.NET** supports the following host key algorithms:
* ssh-ed25519
* ecdsa-sha2-nistp256
* ecdsa-sha2-nistp384
* ecdsa-sha2-nistp521
* rsa-sha2-512
* rsa-sha2-256
* ssh-rsa
* ssh-dss

## Message Authentication Code

**SSH.NET** supports the following MAC algorithms:
* hmac-sha2-256
* hmac-sha2-512
* hmac-sha1
* hmac-sha2-256-etm<span></span>@openssh.com
* hmac-sha2-512-etm<span></span>@openssh.com
* hmac-sha1-etm<span></span>@openssh.com

## Compression

**SSH.NET** supports the following compression algorithms:
* none (default)
* zlib<span></span>@openssh.com

## Framework Support

**SSH.NET** supports the following target frameworks:
* .NETFramework 4.6.2 (and higher)
* .NET Standard 2.0 and 2.1
* .NET 8 (and higher)

## Building the library

The library has no special requirements to build, other than an up-to-date .NET SDK. See also [CONTRIBUTING.md](https://github.com/sshnet/SSH.NET/blob/develop/CONTRIBUTING.md).

## Using Pre-Release NuGet Package

If you need an unreleased bugfix or feature, you can use the Pre-Release NuGet packages from the `develop` branch which are published to the [GitHub NuGet Registry](https://github.com/sshnet/SSH.NET/pkgs/nuget/SSH.NET).
In order to pull packages from the registry you first have to create a Personal Access Token with the `read:packages` permissions. Then add a NuGet Source for SSH.NET:

Note: you may have to add `--store-password-in-clear-text` on non-Windows platforms.

```
dotnet nuget add source --name SSH.NET --username <username> --password <personalaccesstoken> https://nuget.pkg.github.com/sshnet/index.json
```

Then you can add the the package as described [here](https://github.com/sshnet/SSH.NET/pkgs/nuget/SSH.NET).

## Supporting SSH.NET

Do you or your company rely on **SSH.NET** in your projects? If you want to encourage us to keep on going and show us that you appreciate our work, please consider becoming a [sponsor](https://github.com/sponsors/sshnet) through GitHub Sponsors.

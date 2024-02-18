 ![Logo](images/logo/png/SS-NET-icon-h50.png) SSH.NET
=======
SSH.NET is a Secure Shell (SSH-2) library for .NET, optimized for parallelism.

[![Version](https://img.shields.io/nuget/vpre/SSH.NET.svg)](https://www.nuget.org/packages/SSH.NET)
[![NuGet download count](https://img.shields.io/nuget/dt/SSH.NET.svg)](https://www.nuget.org/packages/SSH.NET)
[![Build status](https://ci.appveyor.com/api/projects/status/ih77qu6tap3o92gu/branch/develop?svg=true)](https://ci.appveyor.com/api/projects/status/ih77qu6tap3o92gu/branch/develop)

## Introduction
This project was inspired by **Sharp.SSH** library which was ported from java and it seems like was not supported
for quite some time. This library is a complete rewrite, without any third party dependencies, using parallelism
to achieve the best performance possible.

## Documentation
There is MSDN-style class documentation in a .chm file for each release, which you can find in the Assets section
of the [latest release](https://github.com/sshnet/SSH.NET/releases/latest) page.  Please note that you will need
to [right-click and "unblock"](https://support.microsoft.com/en-us/help/2021383/some-chm-files-may-not-render-properly-on-windows-vista-and-windows-7)
the CHM file after you download it.

Currently (4/18/2020), the documentation is very sparse.  Fortunately, there are a large number of tests in
[Renci.SshNet.Tests](https://github.com/sshnet/SSH.NET/tree/develop/src/Renci.SshNet.Tests) that demonstrate
usage with working code.

If the test for the functionality you would like to see documented is not complete, then you are cordially
invited to read the source, Luke, and highly encouraged to generate a pull request for the implementation of
the missing test once you figure things out.  🤓

## Features
* Execution of SSH command using both synchronous and asynchronous methods
* Return command execution exit status and other information 
* Provide SFTP functionality for both synchronous and asynchronous operations
* Provides SCP functionality
* Provide status report for upload and download sftp operations to allow accurate progress bar implementation 
* Remote, dynamic and local port forwarding 
* Shell/Terminal implementation
* Specify key file pass phrase
* Use multiple key files to authenticate
* Supports publickey, password and keyboard-interactive authentication methods 
* Supports two-factor or higher authentication
* Supports SOCKS4, SOCKS5 and HTTP Proxy

## Encryption Method

**SSH.NET** supports the following encryption methods:
* aes256-ctr
* 3des-cbc
* aes128-cbc
* aes192-cbc
* aes256-cbc
* blowfish-cbc
* twofish-cbc
* twofish192-cbc
* twofish128-cbc
* twofish256-cbc
* arcfour
* arcfour128
* arcfour256
* cast128-cbc
* aes128-ctr
* aes192-ctr

## Key Exchange Method

**SSH.NET** supports the following key exchange methods:
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
* RSA in OpenSSL PEM and ssh.com format
* DSA in OpenSSL PEM and ssh.com format
* ECDSA 256/384/521 in OpenSSL PEM format
* ECDSA 256/384/521, ED25519 and RSA in OpenSSH key format

Private keys can be encrypted using one of the following cipher methods:
* DES-EDE3-CBC
* DES-EDE3-CFB
* DES-CBC
* AES-128-CBC
* AES-192-CBC
* AES-256-CBC

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
* hmac-md5
* hmac-md5-96
* hmac-sha1
* hmac-sha1-96
* hmac-sha2-256
* hmac-sha2-256-96
* hmac-sha2-512
* hmac-sha2-512-96
* hmac-md5-etm<span></span>@openssh.com
* hmac-md5-96-etm<span></span>@openssh.com
* hmac-sha1-etm<span></span>@openssh.com
* hmac-sha1-96-etm<span></span>@openssh.com
* hmac-sha2-256-etm<span></span>@openssh.com
* hmac-sha2-512-etm<span></span>@openssh.com

## Framework Support
**SSH.NET** supports the following target frameworks:
* .NETFramework 4.6.2 (and higher)
* .NET Standard 2.0 and 2.1
* .NET 6 (and higher)

## Usage

### Multi-factor authentication

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

### Verify host identify

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

## Supporting SSH.NET

Do you or your company rely on **SSH.NET** in your projects? If you want to encourage us to keep on going and show us that you appreciate our work, please consider becoming a [sponsor](https://github.com/sponsors/sshnet) through GitHub Sponsors.

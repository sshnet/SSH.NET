#Introduction#
This project was inspired by **Sharp.SSH** library which was ported from java and it seems like was not supported for quite some time. This library is a complete rewrite, without any third party dependencies, using parallelism to achieve the best performance possible.

#Features#
* Execution of SSH command using both synchronous and asynchronous methods
* Return command execution exit status and other information 
* Provide SFTP functionality for both synchronous and asynchronous operations
* Provides SCP functionality
* Provide status report for upload and download sftp operations to allow accurate progress bar implementation 
* Remote, dynamic and local port forwarding 
* Shell/Terminal implementation
* Specify key file pass phrase
* Use multiple key files to authenticate 
* Supports 3des-cbc, aes128-cbc, aes192-cbc, aes256-cbc, aes128-ctr, aes192-ctr, aes256-ctr, blowfish-cbc, cast128-cbc, arcfour and twofish encryptions
* Supports publickey, password and keyboard-interactive authentication methods 
* Supports RSA and DSA private key 
* Supports DES-EDE3-CBC, DES-EDE3-CFB, DES-CBC, AES-128-CBC, AES-192-CBC and AES-256-CBC algorithms for private key encryption
* Supports two-factor or higher authentication
* Supports SOCKS4, SOCKS5 and HTTP Proxy

#Key Exchange Method#

**SSH.NET** supports the following key exchange methods:
* diffie-hellman-group-exchange-sha256
* diffie-hellman-group-exchange-sha1
* diffie-hellman-group14-sha1
* diffie-hellman-group1-sha1

#Message Authentication Code#

**SSH.NET** supports the following MAC algorithms:
* hmac-md5
* hmac-md5-96
* hmac-sha1
* hmac-sha1-96
* hmac-sha2-256
* hmac-sha2-256-96
* hmac-sha2-512
* hmac-sha2-512-96
* hmac-ripemd160
* hmac-ripemd160@openssh.com

#Framework Support#
**SSH.NET** supports the following target frameworks:
* .NET Framework 3.5 
* .NET Framework 4.0 
* Silverlight 4 
* Silverlight 5 
* Windows Phone 7.1 
* Windows Phone 8.0
* Universal Windows Platform 10

In our codebase, we use the following conditional compilation symbols to identity features supported by a given target framework:

Symbol                           | Description
:------------------------------- | :--------------------------------------------------------------------------------
FEATURE_RNG_CSP                  | RandomNumberGenerator can be created using default ctor of RNGCryptoServiceProvider
FEATURE_RNG_CREATE               | RandomNumberGenerator can be created using static Create method on RandomNumberGenerator
FEATURE_HASH_MD5                 |
FEATURE_HASH_SHA1                |
FEATURE_HASH_SHA256              |
FEATURE_HASH_SHA384              |
FEATURE_HASH_SHA512              |
FEATURE_HASH_RIPEMD160           |
FEATURE_HMAC_MD5                 |
FEATURE_HMAC_SHA1                |
FEATURE_HMAC_SHA256              |
FEATURE_HMAC_SHA384              |
FEATURE_HMAC_SHA512              |
FEATURE_HMAC_RIPEMD160           |
FEATURE_THREAD_SLEEP             | Current thread can be suspended using static Sleep method on Thread class
FEATURE_THREAD_TAP               | Task-based Asynchronous Pattern is supported
FEATURE_THREAD_THREADPOOL        | ThreadPool is supported
FEATURE_MEMORYSTREAM_GETBUFFER   | [MemoryStream] () features a [GetBuffer()](https://msdn.microsoft.com/en-us/library/system.io.memorystream.getbuffer.aspx) method which allows access to the byte array from which this stream was created, or the underlying array 
FEATURE_MEMORYSTREAM_TRYGETBUFFER   | [MemoryStream] () features a [TryGetBuffer(out ArraySegment<byte>)](https://msdn.microsoft.com/en-us/library/system.io.memorystream.trygetbuffer.aspx) method which allows access to the byte array from which this stream was created, or the underlying array
FEATURE_STREAM_TAP               | [Stream](https://msdn.microsoft.com/en-us/library/system.io.stream.aspx) supports Task-based Asynchronous Pattern (TAP)
FEATURE_STREAM_EAP               | [Stream](https://msdn.microsoft.com/en-us/library/system.io.stream.aspx) supports Event-based Asynchronous Pattern (EAP)
FEATURE_STREAM_APM               | [Stream](https://msdn.microsoft.com/en-us/library/system.io.stream.aspx) supports Asynchronous Programming Model (APM)
FEATURE_STREAM_SYNC              | [Stream](https://msdn.microsoft.com/en-us/library/system.io.stream.aspx) supports synchronous execution
FEATURE_SOCKET_TAP               | [Socket](https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx) supports Task-based Asynchronous Pattern (TAP)
FEATURE_SOCKET_EAP               | [Socket](https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx) supports Event-based Asynchronous Pattern (EAP)
FEATURE_SOCKET_APM               | [Socket](https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx) supports Asynchronous Programming Model (APM)
FEATURE_SOCKET_SYNC              | [Socket](https://msdn.microsoft.com/en-us/library/system.net.sockets.socket.aspx) supports synchronous execution
FEATURE_DNS_TAP                  | [Dns](https://msdn.microsoft.com/en-us/library/system.net.dns.aspx) supports Task-based Asynchronous Pattern (TAP)
FEATURE_DNS_EAP                  | [Dns](https://msdn.microsoft.com/en-us/library/system.net.dns.aspx) supports Event-based Asynchronous Pattern (EAP)
FEATURE_DNS_APM                  | [Dns](https://msdn.microsoft.com/en-us/library/system.net.dns.aspx) supports Asynchronous Programming Model (APM)
FEATURE_DNS_SYNC                 | [Dns](https://msdn.microsoft.com/en-us/library/system.net.dns.aspx) supports synchronous execution
FEATURE_REFLECTION_TYPEINFO      |

#Building SSH.NET#

Software                          | .NET 3.5 | .NET 4.0 | SL 4 | SL 5 | WP 71 | WP 80 | UAP10
--------------------------------- | :------: | :------: | :--: | :--: | :---: | :---: | :---:
Windows Phone SDK 8.0             |          |          | x    | x    | x     | x     |
Visual Studio 2012 Update 5       | x        | x        | x    | x    | x     | x     |
Visual Studio 2015 Update 2 RC    | x        | x        |      | x    |       | x     | x

[![NDepend](http://download-codeplex.sec.s-msft.com/Download?ProjectName=sshnet&DownloadId=629750)](http://ndepend.com)

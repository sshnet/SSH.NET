#pragma warning disable IDE0005

extern alias LocalSshNet;

global using System.Text;

global using Microsoft.VisualStudio.TestTools.UnitTesting;

global using IntegrationTests.TestsFixtures;

// The testcontainers library uses SSH.NET, so we have two versions of SSH.NET in the project.
// We need to explicitly choose which version we want to test.
// To avoid problems, we import all namespaces.
global using LocalSshNet::Renci.SshNet;
global using LocalSshNet::Renci.SshNet.Abstractions;
global using LocalSshNet::Renci.SshNet.Channels;
global using LocalSshNet::Renci.SshNet.Common;
global using LocalSshNet::Renci.SshNet.Compression;
global using LocalSshNet::Renci.SshNet.Connection;
global using LocalSshNet::Renci.SshNet.Messages;
global using LocalSshNet::Renci.SshNet.Messages.Authentication;
global using LocalSshNet::Renci.SshNet.Messages.Connection;
global using LocalSshNet::Renci.SshNet.Messages.Transport;
global using LocalSshNet::Renci.SshNet.NetConf;
global using LocalSshNet::Renci.SshNet.Security;
global using LocalSshNet::Renci.SshNet.Security.Chaos;
global using LocalSshNet::Renci.SshNet.Security.Chaos.NaCl;
global using LocalSshNet::Renci.SshNet.Security.Chaos.NaCl.Internal;
global using LocalSshNet::Renci.SshNet.Security.Cryptography;
global using LocalSshNet::Renci.SshNet.Security.Cryptography.Ciphers;
global using LocalSshNet::Renci.SshNet.Security.Org;
global using LocalSshNet::Renci.SshNet.Security.Org.BouncyCastle;

global using LocalSshNet::Renci.SshNet.Sftp;



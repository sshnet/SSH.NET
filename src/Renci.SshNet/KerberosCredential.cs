#nullable enable
using System.Net;

using Renci.SshNet.Common;

namespace Renci.SshNet
{
    /// <summary>
    /// Represents credential for Kerberos authentication.
    /// </summary>
    public sealed class KerberosCredential
    {
        internal NetworkCredential? NetworkCredential { get; }
        internal bool DelegateCredential { get; }
        internal string? TargetName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="KerberosCredential"/> class.
        /// </summary>
        /// <remarks>
        /// The credential specified is used for the Kerberos authentication process. This can either be the same or
        /// different from the username specified through <c>ConnectionInfo.Username</c>. The client settings username
        /// is the target login user the SSH service is meant to run as, whereas the credential is the Kerberos
        /// principal used for authentication. The rules for how a Kerberos principal maps to the target user is defined by
        /// the SSH service itself. For example on Windows the username should be the same but on Linux the mapping can be
        /// done through a <c>.k5login</c> file in the target user's home directory.
        ///
        /// If the credential is <see langword="null"/>, the Kerberos authentication will be done using a cached ticket.
        /// For Windows, this is the current thread's identity (typically logon user) will be used.
        /// For Unix/Linux, this will use the Kerberos credential cache principal, which may be managed using the
        /// <c>kinit</c> command. If there is no available cache credential, the authentication will fail.
        ///
        /// Credentials can only be delegated if the Kerberos ticket retrieved from the KDC is marked as forwardable.
        /// Windows hosts will always retrieve a forwardable ticket but non-Windows hosts may not. When using an explicit
        /// credential, make sure that 'forwardable = true' is set in the krb5.conf file so that .NET will request a
        /// forwardable ticket required for delegation. When using a cached ticket, make sure that when the ticket was
        /// retrieved it was retrieved with the forwardable flag. If the ticket is not forwardable, the authentication will
        /// still work but the ticket will not be delegated.
        /// </remarks>
        /// <param name="credential">The credentials to use for the Kerberos authentication exchange. Set to null to use a cached ticket.</param>
        /// <param name="delegateCredential">Allows the SSH server to delegate the user on remote systems.</param>
        /// <param name="targetName">Override the service principal name (SPN), default uses <c>host/&lt;ConnectionInfo.Host&gt;</c>.</param>
        public KerberosCredential(NetworkCredential? credential = null, bool delegateCredential = false, string? targetName = null)
        {
            if (!string.IsNullOrWhiteSpace(credential?.UserName))
            {
                ThrowHelper.ThrowIfNullOrEmpty(credential!.Password);
            }

            NetworkCredential = credential;
            DelegateCredential = delegateCredential;
            TargetName = targetName;
        }
    }
}

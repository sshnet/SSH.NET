namespace Renci.SshNet
{
    /// <summary>
    /// Specifies the way host names will be resolved when conecting through a proxy.
    /// </summary>
    public enum HostResolutionMode
    {
        /// <summary>The host name is resolved by the client and the host IP is sent to the proxy.</summary>
        ResolvedLocally,

        /// <summary>The host name is sent to the proxy and resolved later.</summary>
        ResolvedByProxy
    }
}
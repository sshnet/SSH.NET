using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Math.Field
{
    internal interface IExtensionField
        : IFiniteField
    {
        IFiniteField Subfield { get; }

        int Degree { get; }
    }
}

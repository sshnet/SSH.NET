using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Crypto
{
    internal class AsymmetricCipherKeyPair
    {
        private readonly AsymmetricKeyParameter publicParameter;
        private readonly AsymmetricKeyParameter privateParameter;

        public AsymmetricCipherKeyPair(
            AsymmetricKeyParameter    publicParameter,
            AsymmetricKeyParameter    privateParameter)
        {
			if (publicParameter.IsPrivate)
				throw new ArgumentException("Expected a public key", "publicParameter");
			if (!privateParameter.IsPrivate)
				throw new ArgumentException("Expected a private key", "privateParameter");

			this.publicParameter = publicParameter;
            this.privateParameter = privateParameter;
        }

        public AsymmetricKeyParameter Public
        {
            get { return publicParameter; }
        }

        public AsymmetricKeyParameter Private
        {
            get { return privateParameter; }
        }
    }
}
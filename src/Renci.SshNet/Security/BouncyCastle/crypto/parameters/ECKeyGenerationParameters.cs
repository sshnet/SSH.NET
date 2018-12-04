using Renci.SshNet.Security.Org.BouncyCastle.Security;

namespace Renci.SshNet.Security.Org.BouncyCastle.Crypto.Parameters
{
    internal class ECKeyGenerationParameters
		: KeyGenerationParameters
    {
        private readonly ECDomainParameters domainParams;

		public ECKeyGenerationParameters(
			ECDomainParameters	domainParameters,
			SecureRandom		random)
			: base(random, domainParameters.N.BitLength)
        {
            this.domainParams = domainParameters;
        }

		public ECDomainParameters DomainParameters
        {
			get { return domainParams; }
        }
    }
}
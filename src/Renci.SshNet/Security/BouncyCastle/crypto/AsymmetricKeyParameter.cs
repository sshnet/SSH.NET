namespace Renci.SshNet.Security.Org.BouncyCastle.Crypto
{
    internal abstract class AsymmetricKeyParameter
    {
        private readonly bool privateKey;

        protected AsymmetricKeyParameter(
            bool privateKey)
        {
            this.privateKey = privateKey;
        }

		public bool IsPrivate
        {
            get { return privateKey; }
        }

		public override bool Equals(
			object obj)
		{
			AsymmetricKeyParameter other = obj as AsymmetricKeyParameter;

			if (other == null)
			{
				return false;
			}

			return Equals(other);
		}

		protected bool Equals(
			AsymmetricKeyParameter other)
		{
			return privateKey == other.privateKey;
		}

		public override int GetHashCode()
		{
			return privateKey.GetHashCode();
		}
    }
}
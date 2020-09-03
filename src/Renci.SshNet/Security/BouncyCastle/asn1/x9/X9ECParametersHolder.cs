namespace Renci.SshNet.Security.Org.BouncyCastle.Asn1.X9
{
	internal abstract class X9ECParametersHolder
	{
		private X9ECParameters parameters;

		public X9ECParameters Parameters
		{
			get
			{
                lock (this)
                {
                    if (parameters == null)
                    {
                        parameters = CreateParameters();
                    }

                    return parameters;
                }
            }
        }

		protected abstract X9ECParameters CreateParameters();
	}
}
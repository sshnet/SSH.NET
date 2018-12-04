using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Security
{
#if !(NETCF_1_0 || NETCF_2_0 || SILVERLIGHT || PORTABLE)
    [Serializable]
#endif
    internal class SecurityUtilityException
		: Exception
    {
        /**
        * base constructor.
        */
        public SecurityUtilityException()
        {
        }

		/**
         * create a SecurityUtilityException with the given message.
         *
         * @param message the message to be carried with the exception.
         */
        public SecurityUtilityException(
            string message)
			: base(message)
        {
        }

		public SecurityUtilityException(
            string		message,
            Exception	exception)
			: base(message, exception)
        {
        }
    }
}

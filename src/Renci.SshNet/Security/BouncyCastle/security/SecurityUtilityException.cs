using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Security
{
#if FEATURE_BINARY_SERIALIZATION
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

using System;

namespace Renci.SshNet.Security.Org.BouncyCastle.Utilities
{
    /**
     * Exception to be thrown on a failure to reset an object implementing Memoable.
     * <p>
     * The exception extends InvalidCastException to enable users to have a single handling case,
     * only introducing specific handling of this one if required.
     * </p>
     */
    internal class MemoableResetException
        : InvalidCastException
    {
        /**
         * Basic Constructor.
         *
         * @param msg message to be associated with this exception.
         */
        public MemoableResetException(string msg)
            : base(msg)
        {
        }
    }

}


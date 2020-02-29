namespace Renci.SshNet.Security.Org.BouncyCastle.Math.EC.Multiplier
{
    /**
    * Class holding precomputation data for the WNAF (Window Non-Adjacent Form)
    * algorithm.
    */
    internal class WNafPreCompInfo
        : PreCompInfo 
    {
        /**
         * Array holding the precomputed <code>ECPoint</code>s used for a Window
         * NAF multiplication.
         */
        protected ECPoint[] m_preComp = null;

        /**
         * Array holding the negations of the precomputed <code>ECPoint</code>s used
         * for a Window NAF multiplication.
         */
        protected ECPoint[] m_preCompNeg = null;

        /**
         * Holds an <code>ECPoint</code> representing Twice(this). Used for the
         * Window NAF multiplication to create or extend the precomputed values.
         */
        protected ECPoint m_twice = null;

        public virtual ECPoint[] PreComp
        {
            get { return m_preComp; }
            set { this.m_preComp = value; }
        }

        public virtual ECPoint[] PreCompNeg
        {
            get { return m_preCompNeg; }
            set { this.m_preCompNeg = value; }
        }

        public virtual ECPoint Twice
        {
            get { return m_twice; }
            set { this.m_twice = value; }
        }
    }
}

using System;
using System.Text.RegularExpressions;

namespace Renci.SshNet
{
    /// <summary>
    /// Specifies behavior for expected expression
    /// </summary>
    public class ExpectAction
    {
        /// <summary>
        /// Gets the expected regular expression.
        /// </summary>
        public Regex Expect { get; private set; }

        /// <summary>
        /// Gets the action to perform when expected expression is found.
        /// </summary>
        public Action<string> Action { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectAction"/> class.
        /// </summary>
        /// <param name="expect">The expect regular expression.</param>
        /// <param name="action">The action to perform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expect"/> or <paramref name="action"/> is <c>null</c>.</exception>
        public ExpectAction(Regex expect, Action<string> action)
        {
            if (expect == null)
                throw new ArgumentNullException("expect");

            if (action == null)
                throw new ArgumentNullException("action");

            Expect = expect;
            Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectAction"/> class.
        /// </summary>
        /// <param name="expect">The expect expression.</param>
        /// <param name="action">The action to perform.</param>
        /// <exception cref="ArgumentNullException"><paramref name="expect"/> or <paramref name="action"/> is <c>null</c>.</exception>
        public ExpectAction(string expect, Action<string> action)
        {
            if (expect == null)
                throw new ArgumentNullException("expect");

            if (action == null)
                throw new ArgumentNullException("action");

            Expect = new Regex(Regex.Escape(expect));
            Action = action;
        }
    }
}

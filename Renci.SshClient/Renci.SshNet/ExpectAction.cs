using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public ExpectAction(Regex expect, Action<string> action)
        {
            this.Expect = expect;
            this.Action = action;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectAction"/> class.
        /// </summary>
        /// <param name="expect">The expect expression.</param>
        /// <param name="action">The action to perform.</param>
        public ExpectAction(string expect, Action<string> action)
        {
            this.Expect = new Regex(Regex.Escape(expect));
            this.Action = action;
        }
    }
}

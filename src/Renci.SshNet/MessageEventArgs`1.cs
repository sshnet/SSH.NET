﻿using System;

namespace Renci.SshNet
{
    /// <summary>
    /// Provides data for message events.
    /// </summary>
    /// <typeparam name="T">Message type.</typeparam>
    public class MessageEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        public T Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageEventArgs{T}"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <see langword="null"/>.</exception>
        public MessageEventArgs(T message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Message = message;
        }
    }
}

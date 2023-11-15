using System;
using System.Threading;

namespace Renci.SshNet
{
    internal sealed class ForwardedPortStatus
    {
        public static readonly ForwardedPortStatus Stopped = new ForwardedPortStatus(1, "Stopped");
        public static readonly ForwardedPortStatus Stopping = new ForwardedPortStatus(2, "Stopping");
        public static readonly ForwardedPortStatus Started = new ForwardedPortStatus(3, "Started");
        public static readonly ForwardedPortStatus Starting = new ForwardedPortStatus(4, "Starting");

        private readonly int _value;
        private readonly string _name;

        private ForwardedPortStatus(int value, string name)
        {
            _value = value;
            _name = name;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is not ForwardedPortStatus forwardedPortStatus)
            {
                return false;
            }

            return forwardedPortStatus._value == _value;
        }

#pragma warning disable S3875 // "operator==" should not be overloaded on reference types
        public static bool operator ==(ForwardedPortStatus left, ForwardedPortStatus right)
#pragma warning restore S3875 // "operator==" should not be overloaded on reference types
        {
            // check if lhs is null
            if (left is null)
            {
                // check if both lhs and rhs are null
                return right is null;
            }

            return left.Equals(right);
        }

        public static bool operator !=(ForwardedPortStatus left, ForwardedPortStatus right)
        {
            return !(left==right);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override string ToString()
        {
            return _name;
        }

        /// <summary>
        /// Returns a value indicating whether <paramref name="status"/> has been changed to <see cref="Stopping"/>.
        /// </summary>
        /// <param name="status">The status to transition from.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="status"/> has been changed to <see cref="Stopping"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">Cannot transition <paramref name="status"/> to <see cref="Stopping"/>.</exception>
        /// <remarks>
        /// While a transition from <see cref="Stopped"/> to <see cref="Stopping"/> is not possible, this method will
        /// return <see langword="false"/> for any such attempts.  This is related to concurrency.
        /// </remarks>
        public static bool ToStopping(ref ForwardedPortStatus status)
        {
            // attempt to transition from Started to Stopping
            var previousStatus = Interlocked.CompareExchange(ref status, Stopping, Started);
            if (previousStatus == Stopping || previousStatus == Stopped)
            {
                // status is already Stopping or Stopped, so no transition to Stopping is necessary
                return false;
            }

            // we've successfully transitioned from Started to Stopping
            if (status == Stopping)
            {
                return true;
            }

            // attempt to transition from Starting to Stopping
            previousStatus = Interlocked.CompareExchange(ref status, Stopping, Starting);
            if (previousStatus == Stopping || previousStatus == Stopped)
            {
                // status is already Stopping or Stopped, so no transition to Stopping is necessary
                return false;
            }

            // we've successfully transitioned from Starting to Stopping
            if (status == Stopping)
            {
                return true;
            }

            // there's no valid transition from status to Stopping
            throw new InvalidOperationException(string.Format("Forwarded port cannot transition from '{0}' to '{1}'.",
                                                              previousStatus,
                                                              Stopping));
        }

        /// <summary>
        /// Returns a value indicating whether <paramref name="status"/> has been changed to <see cref="Starting"/>.
        /// </summary>
        /// <param name="status">The status to transition from.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="status"/> has been changed to <see cref="Starting"/>; otherwise, <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">Cannot transition <paramref name="status"/> to <see cref="Starting"/>.</exception>
        /// <remarks>
        /// While a transition from <see cref="Started"/> to <see cref="Starting"/> is not possible, this method will
        /// return <see langword="false"/> for any such attempts.  This is related to concurrency.
        /// </remarks>
        public static bool ToStarting(ref ForwardedPortStatus status)
        {
            // attemp to transition from Stopped to Starting
            var previousStatus = Interlocked.CompareExchange(ref status, Starting, Stopped);
            if (previousStatus == Starting || previousStatus == Started)
            {
                // port is already Starting or Started, so no transition to Starting is necessary
                return false;
            }

            // we've successfully transitioned from Stopped to Starting
            if (status == Starting)
            {
                return true;
            }

            // there's no valid transition from status to Starting
            throw new InvalidOperationException(string.Format("Forwarded port cannot transition from '{0}' to '{1}'.",
                                                              previousStatus,
                                                              Starting));
        }
    }
}

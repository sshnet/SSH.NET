using System;
using System.Threading;

namespace Renci.SshNet
{
    internal class ForwardedPortStatus
    {
        private readonly int _value;
        private readonly string _name;

        public static readonly ForwardedPortStatus Stopped = new ForwardedPortStatus(1, "Stopped");
        public static readonly ForwardedPortStatus Stopping = new ForwardedPortStatus(2, "Stopping");
        public static readonly ForwardedPortStatus Started = new ForwardedPortStatus(3, "Started");
        public static readonly ForwardedPortStatus Starting = new ForwardedPortStatus(4, "Starting");

        private ForwardedPortStatus(int value, string name)
        {
            _value = value;
            _name = name;
        }

        public override bool Equals(object other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            var forwardedPortStatus = other as ForwardedPortStatus;
            if (forwardedPortStatus == null)
                return false;

            return forwardedPortStatus._value == _value;
        }

        public static bool operator ==(ForwardedPortStatus left, ForwardedPortStatus right)
        {
            // check if lhs is null
            if (ReferenceEquals(left, null))
            {
                // check if both lhs and rhs are null
                return (ReferenceEquals(right, null));
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
        /// <c>true</c> if <paramref name="status"/> has been changed to <see cref="Stopping"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">Cannot transition <paramref name="status"/> to <see cref="Stopping"/>.</exception>
        /// <remarks>
        /// While a transition from <see cref="Stopped"/> to <see cref="Stopping"/> is not possible, this method will
        /// return <c>false</c> for any such attempts.  This is related to concurrency.
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
                return true;

            // attempt to transition from Starting to Stopping
            previousStatus = Interlocked.CompareExchange(ref status, Stopping, Starting);
            if (previousStatus == Stopping || previousStatus == Stopped)
            {
                // status is already Stopping or Stopped, so no transition to Stopping is necessary
                return false;
            }

            // we've successfully transitioned from Starting to Stopping
            if (status == Stopping)
                return true;

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
        /// <c>true</c> if <paramref name="status"/> has been changed to <see cref="Starting"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="InvalidOperationException">Cannot transition <paramref name="status"/> to <see cref="Starting"/>.</exception>
        /// <remarks>
        /// While a transition from <see cref="Started"/> to <see cref="Starting"/> is not possible, this method will
        /// return <c>false</c> for any such attempts.  This is related to concurrency.
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
                return true;

            // there's no valid transition from status to Starting
            throw new InvalidOperationException(string.Format("Forwarded port cannot transition from '{0}' to '{1}'.",
                                                              previousStatus,
                                                              Starting));
        }
    }
}

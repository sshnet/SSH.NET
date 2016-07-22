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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var forwardedPortStatus = obj as ForwardedPortStatus;
            if (forwardedPortStatus == null)
                return false;

            return forwardedPortStatus._value == _value;
        }

        public static bool operator ==(ForwardedPortStatus c1, ForwardedPortStatus c2)
        {
            // check if lhs is null
            if (ReferenceEquals(c1, null))
            {
                // check if both lhs and rhs are null
                return (ReferenceEquals(c2, null));
            }

            return c1.Equals(c2);
        }

        public static bool operator !=(ForwardedPortStatus c1, ForwardedPortStatus c2)
        {
            return !(c1==c2);
        }

        public override int GetHashCode()
        {
            return _value;
        }

        public override string ToString()
        {
            return _name;
        }

        public static bool ToStopping(ref ForwardedPortStatus status)
        {
            // attempt to transition from Started to Stopping
            var previousStatus = Interlocked.CompareExchange(ref status, Stopping, Started);
            if (previousStatus == Stopping || previousStatus == Stopped)
            {
                // status is already Stopping or Stopped, so no transition to Stopped is necessary
                return false;
            }

            // we've successfully transitioned from Started to Stopping
            if (status == Stopping)
                return true;

            // attempt to transition from Starting to Stopping
            previousStatus = Interlocked.CompareExchange(ref status, Stopping, Starting);
            if (previousStatus == Stopping || previousStatus == Stopped)
            {
                // status is already Stopping or Stopped, so no transition to Stopped is necessary
                return false;
            }

            // we've successfully transitioned from Starting to Stopping
            return status == Stopping;
        }

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

            // there's no valid transition from Stopping to Starting
            throw new InvalidOperationException("Forwarded port is stopping.");
        }
    }
}

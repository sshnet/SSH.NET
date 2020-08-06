using System;

namespace Renci.SshNet
{
    /// <summary>
    /// 
    /// </summary>
    public class TypeWithPriority
    {
        /// <summary>
        /// 
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="priority"></param>
        public TypeWithPriority(Type type, int priority)
        {
            Type = type;
            Priority = priority;
        }
    }
}

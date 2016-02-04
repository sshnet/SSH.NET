namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "env" type channel request information
    /// </summary>
    internal class EnvironmentVariableRequestInfo : RequestInfo
    {
#if TUNING
        private byte[] _variableName;
        private byte[] _variableValue;
#endif

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string NAME = "env";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return NAME; }
        }

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
#if TUNING
        public string VariableName
        {
            get { return Utf8.GetString(_variableName, 0, _variableName.Length); }
        }
#else
        public string VariableName { get; set; }
#endif

        /// <summary>
        /// Gets or sets the variable value.
        /// </summary>
        /// <value>
        /// The variable value.
        /// </value>
#if TUNING
        public string VariableValue
        {
            get { return Utf8.GetString(_variableValue, 0, _variableValue.Length); }
        }
#else
        public string VariableValue { get; set; }
#endif

#if TUNING
        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        /// <value>
        /// The size of the messages in bytes.
        /// </value>
        protected override int BufferCapacity
        {
            get
            {
                var capacity = base.BufferCapacity;
                capacity += 4; // VariableName length
                capacity += _variableName.Length; // VariableName
                capacity += 4; // VariableValue length
                capacity += _variableValue.Length; // VariableValue
                return capacity;
            }
        }
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariableRequestInfo"/> class.
        /// </summary>
        public EnvironmentVariableRequestInfo()
        {
            WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariableRequestInfo"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableValue">The variable value.</param>
        public EnvironmentVariableRequestInfo(string variableName, string variableValue)
            : this()
        {
#if TUNING
            _variableName = Utf8.GetBytes(variableName);
            _variableValue = Utf8.GetBytes(variableValue);
#else
            VariableName = variableName;
            VariableValue = variableValue;
#endif
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

#if TUNING
            _variableName = ReadBinary();
            _variableValue = ReadBinary();
#else
            VariableName = ReadString();
            VariableValue = ReadString();
#endif
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

#if TUNING
            WriteBinaryString(_variableName);
            WriteBinaryString(_variableValue);
#else
            Write(VariableName);
            Write(VariableValue);
#endif
        }
    }
}

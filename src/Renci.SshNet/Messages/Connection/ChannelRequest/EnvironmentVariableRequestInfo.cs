namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "env" type channel request information
    /// </summary>
    internal class EnvironmentVariableRequestInfo : RequestInfo
    {
        private byte[] _variableName;
        private byte[] _variableValue;

        /// <summary>
        /// Channel request name
        /// </summary>
        public const string Name = "env";

        /// <summary>
        /// Gets the name of the request.
        /// </summary>
        /// <value>
        /// The name of the request.
        /// </value>
        public override string RequestName
        {
            get { return Name; }
        }

        /// <summary>
        /// Gets or sets the name of the variable.
        /// </summary>
        /// <value>
        /// The name of the variable.
        /// </value>
        public string VariableName
        {
            get { return Utf8.GetString(_variableName, 0, _variableName.Length); }
        }

        /// <summary>
        /// Gets or sets the variable value.
        /// </summary>
        /// <value>
        /// The variable value.
        /// </value>
        public string VariableValue
        {
            get { return Utf8.GetString(_variableValue, 0, _variableValue.Length); }
        }

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
            _variableName = Utf8.GetBytes(variableName);
            _variableValue = Utf8.GetBytes(variableValue);
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            _variableName = ReadBinary();
            _variableValue = ReadBinary();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            WriteBinaryString(_variableName);
            WriteBinaryString(_variableValue);
        }
    }
}

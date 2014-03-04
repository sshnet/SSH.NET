namespace Renci.SshNet.Messages.Connection
{
    /// <summary>
    /// Represents "env" type channel request information
    /// </summary>
    internal class EnvironmentVariableRequestInfo : RequestInfo
    {
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
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets the variable value.
        /// </summary>
        /// <value>
        /// The variable value.
        /// </value>
        public string VariableValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariableRequestInfo"/> class.
        /// </summary>
        public EnvironmentVariableRequestInfo()
        {
            this.WantReply = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnvironmentVariableRequestInfo"/> class.
        /// </summary>
        /// <param name="variableName">Name of the variable.</param>
        /// <param name="variableValue">The variable value.</param>
        public EnvironmentVariableRequestInfo(string variableName, string variableValue)
            : this()
        {
            this.VariableName = variableName;
            this.VariableValue = variableValue;
        }

        /// <summary>
        /// Called when type specific data need to be loaded.
        /// </summary>
        protected override void LoadData()
        {
            base.LoadData();

            this.VariableName = this.ReadString();
            this.VariableValue = this.ReadString();
        }

        /// <summary>
        /// Called when type specific data need to be saved.
        /// </summary>
        protected override void SaveData()
        {
            base.SaveData();

            this.Write(this.VariableName);
            this.Write(this.VariableValue);
        }
    }
}

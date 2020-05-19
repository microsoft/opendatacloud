namespace Msr.Odr.Model
{
    /// <summary>
    /// Represents a strongly-typed error message
    /// </summary>
    public class Error
    {
        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        /// <value>The code.</value>
        public string Code
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        /// <value>The message.</value>
        public string Message
		{
			get;
			set;
		}
    }
}

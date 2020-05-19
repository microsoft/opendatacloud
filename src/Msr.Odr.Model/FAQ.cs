using System;

namespace Msr.Odr.Model
{
    /// <summary>
    /// A Frequently Asked Question
    /// </summary>
    public class FAQ
	{
        /// <summary>
        /// Gets or sets the unique identifier for the FAQ.
        /// </summary>
        /// <value>The identifier.</value>
        public Guid Id
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the title of the FAQ
        /// </summary>
        /// <value>The title.</value>
        public string Title
		{
			get;
			set;
		}

        /// <summary>
        /// Gets or sets the HTML content of the FAQ
        /// </summary>
        /// <value>The title.</value>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// Value to determine the display order or the FAQ
        /// </summary>
        /// <value>The order.</value>
        public double Order
        {
            get;
            set;
        }
    }
}
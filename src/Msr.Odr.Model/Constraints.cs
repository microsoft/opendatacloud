
namespace Msr.Odr.Model
{
	/// <summary>
	/// Global constraints for model values.
	/// </summary>
	public static class Constraints
    {
		/// <summary>
		/// The maximum length for a facet (tags, licenses, extensions) value
		/// </summary>
		public const int FacetValue = 64;

		/// <summary>
		/// The maximum number of facet values in a search request
		/// </summary>
		public const int MaxFacetValues = 10;

		/// <summary>
		/// The maximum facets in a search request
		/// </summary>
		public const int MaxFacets = 4;

		/// <summary>
		/// The plain text search criteria length
		/// </summary>
		public const int TermLength = 256;

		/// <summary>
		/// The page size
		/// </summary>
		public const int PageSize = 2;

		/// <summary>
		/// The maximum length for long names (128)
		/// </summary>
		public const int LongName = 128;

		/// <summary>
		/// The maximum length for contact name
		/// </summary>
		public const int ContactNameLength = 128;

		/// <summary>
		/// The maximum length for contact information
		/// </summary>
		public const int ContactInfoLength = 200;

		/// <summary>
		/// The maximum length for medium names (64)
		/// </summary>
		public const int MedName = 64;

		/// <summary>
		/// The maximum length for short names (32)
		/// </summary>
		public const int ShortName = 32;

		/// <summary>
		/// The maximum length for a medium description (5000)
		/// </summary>
		public const int MedDescription = 5000;

        /// <summary>
        /// The maximum length for a version text (50)
        /// </summary>
        public const int VersionLength = 50;

        /// <summary>
        /// The maximum file name length
        /// </summary>
        public const int MaxFileNameLength = 256;

        /// <summary>
        /// The maximum license name length
        /// </summary>
        public const int MaxLicenseNameLength = 256;

        /// <summary>
        /// The maximum url length
        /// </summary>
        public const int UrlLength = 1024;

        /// <summary>
        ///  The maximum number of tags
        /// </summary>
        public const int MaxTags = 10;

        /// <summary>
        ///  The maximum number of file types
        /// </summary>
        public const int MaxFileTypes = 10;

        /// <summary>
        /// The maximum length for Digital Object Identifier (255)
        /// </summary>
        public const int DigitalObjectIdentifierLength = 255;


        /// <summary>
        /// The maximum length of pasted Html License Content
        /// </summary>
        public const int OtherLicenseContentHtml = 2000000;

    }
}

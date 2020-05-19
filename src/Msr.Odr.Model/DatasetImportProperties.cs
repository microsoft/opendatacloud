using System;

namespace Msr.Odr.Model
{
    /// <summary>
    /// Input to the Dataset Transfer utility.
    /// </summary>
    public class DatasetImportProperties
    {
        public Guid Id { get; set; }
        public string DatasetName { get; set; }
        public string AccountName { get; set; }
        public string ContainerName { get; set; }
        public string AccessToken { get; set; }
    }
}

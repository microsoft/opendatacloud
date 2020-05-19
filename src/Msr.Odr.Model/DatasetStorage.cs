using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Msr.Odr.Model.UserData;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msr.Odr.Model
{
    /// <summary>
    /// Information about a dataset storage.
    /// </summary>
    /// <remarks>
    /// Currently assumes this is blob storage, but may use Data Lake in the future.
    /// </remarks>
    public class DatasetStorage
    {
        /// <summary>
        /// Id of dataset.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of dataset.
        /// </summary>
        public string DatasetName { get; set; }

        /// <summary>
        /// The storage account name.
        /// </summary>
        public string AccountName { get; set; }

        /// <summary>
        /// The name of the container within the storage account.
        /// </summary>
        public string ContainerName { get; set; }
    }
}

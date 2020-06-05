// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using Msr.Odr.Model.UserData;

namespace Msr.Odr.Services
{
    public class PublishUpdatedDatasetResult
    {
        /// <summary>
        /// True or false whether the dataset edit was actually published.
        /// </summary>
        public bool IsPublished { get; set; }

        /// <summary>
        /// The status of the dataset edit when it was published.
        /// </summary>
        public DatasetEditStatus UsingStatus { get; set; }

        /// <summary>
        /// True if the batch operation to catalog/archive the dataset jobs should be initiated.
        /// </summary>
        public bool ShouldQueueBatchOperation => UsingStatus == DatasetEditStatus.ContentsModified;

        /// <summary>
        /// Unique id of the dataset.
        /// </summary>
        public Guid DatasetId { get; set; }

        /// <summary>
        /// Name of the dataset.
        /// </summary>
        public string DatasetName { get; set; }
    }
}

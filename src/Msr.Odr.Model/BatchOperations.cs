// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Msr.Odr.Model
{
    public class BatchOperationsSystemStatus
    {
        /// <summary>
        /// Gets the number of tasks in the active state.
        /// </summary>
        public int Active { get; set; }

        /// <summary>
        /// Gets the number of tasks in the completed state.
        /// </summary>
        public int Completed { get; set; }

        /// <summary>
        /// Gets the number of tasks which failed. A task fails if its result (found in the executionInfo property) is 'failure'.
        /// </summary>
        public int Failed { get; set; }

        /// <summary>
        /// Gets the number of tasks in the running or preparing state.
        /// </summary>
        public int Running { get; set;  }

        /// <summary>
        /// Gets the number of tasks which succeeded. A task succeeds if its result (found in the executionInfo property)
        /// is 'success'.
        /// </summary>
        public int Succeeded { get; set; }

        /// <summary>
        /// Whether or not Azure Batch has validated these numbers.
        /// </summary>
        public string ValidationStatus { get; set; }
    }

    /// <summary>
    /// Information about a single batch operation (a single Azure Batch task).
    /// </summary>
    public class BatchOperation
    {
        /// <summary>
        /// The unique Id of the task.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The display name of the task.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The current state of the task.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Date/Time the task was created.
        /// </summary>
        public DateTime CreationTime { get; set; }
    }

    /// <summary>
    /// Information about batch operations.
    /// </summary>
    public class BatchOperations
    {
        /// <summary>
        /// System status for batch operations.
        /// </summary>
        public BatchOperationsSystemStatus Status { get; set; }

        /// <summary>
        /// The page number of the collection of batch operations (tasks).
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The page size of the collection of batch operations (tasks).
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The collection of batch operations (tasks).
        /// </summary>
        public ICollection<BatchOperation> Operations { get; set; }
    }

    /// <summary>
    /// Output from batch operation
    /// </summary>
    public class BatchOperationOutput
    {
        /// <summary>
        /// Standard output from batch operation.
        /// </summary>
        public string Text { get; set; }
    }
}

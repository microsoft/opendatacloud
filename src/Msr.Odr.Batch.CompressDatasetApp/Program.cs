// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Msr.Odr.Batch.Shared;

namespace Msr.Odr.Batch.CompressDatasetApp
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // To run locally, uncomment these lines:
            //BatchRunner.StoreBatchOutput = false;

            return await BatchRunner.Run<CompressDatasetTask>(async (task) =>
            {
                var options = CompressDatasetTaskOptions.Parse(args);
                return await task.Run(options);
            });
        }
    }
}

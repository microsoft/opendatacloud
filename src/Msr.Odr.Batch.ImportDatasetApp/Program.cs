using System;
using System.Threading.Tasks;
using Msr.Odr.Batch.Shared;

namespace Msr.Odr.Batch.ImportDatasetApp
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // To run locally, uncomment these lines:
            //BatchRunner.StoreBatchOutput = false;
            //BatchRunner.QueueJobs = false;

            return await BatchRunner.Run<ImportTask>(async (task) =>
            {
                var options = ImportTaskOptions.Parse(args);
                return await task.Run(options);
            });
        }
    }
}

using System;

namespace Msr.Odr.Batch.CompressDatasetApp
{
    public class CompressDatasetTaskOptions
    {
        public static CompressDatasetTaskOptions Parse(string[] args)
        {
            if (args.Length == 0 || !Guid.TryParse(args[0], out var guid))
            {
                throw new ArgumentException("Command line did not include expected Dataset Id.");
            }

            return new CompressDatasetTaskOptions
            {
                DatasetId = guid
            };
        }

        public Guid DatasetId { get; set; }
    }
}

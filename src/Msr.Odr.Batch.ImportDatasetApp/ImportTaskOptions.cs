// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;

namespace Msr.Odr.Batch.ImportDatasetApp
{
    public class ImportTaskOptions
    {
        public static ImportTaskOptions Parse(string[] args)
        {
            if (args.Length == 0 || !Guid.TryParse(args[0], out var guid))
            {
                throw new ArgumentException("Command line did not include expected Nomination Id.");
            }

            return new ImportTaskOptions
            {
                NominationId = guid
            };
        }

        public Guid NominationId { get; set; }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading.Tasks;
using DatasetUtil.Components;

namespace DatasetUtil
{
    public class Program
    {
        private static Logger Log => Logger.Instance;

        static async Task<int> Main(string[] args)
        {
            try
            {
                Log.Add("MSR ODR Dataset Import Utility (v1.0)").Add();
                CommandArgs.Initialize(args);
                return await new DatasetImport().Run();
            }
            catch (Exception ex)
            {
                Log.Add().Error("Application Error", ex);
                return 1;
            }
        }
    }
}

// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using McMaster.Extensions.CommandLineUtils;

namespace Msr.Odr.Admin
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                Startup.Initialize();
                var app = new CommandLineApplication();
                Commands.MainCommand.Configure(app);
                app.Execute(args);
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 1;
            }
		}
    }
}
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Msr.Odr.IntegrationTests.Setup
{
    public static class TestUtils
    {
        public static async Task ExecAndCleanup(Func<Stack<Func<Task>>, Task> execFn)
        {
            var cleanupQueue = new Stack<Func<Task>>();
            try
            {
                await execFn(cleanupQueue);
            }
            finally
            {
                while (cleanupQueue.Count > 0)
                {
                    var cleanup = cleanupQueue.Pop();
                    await cleanup();
                }
            }
        }

        public static async Task Retry(Func<Task<bool>> fn, int delaySeconds = 5, int totalSeconds = 30)
        {
            int retries = totalSeconds / delaySeconds;
            bool success = false;
            for (int i = 0; !success && i < retries; i++)
            {
                if (i > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                }

                try
                {
                    success = await fn();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Retry #{i + 1} - {ex.Message}");
                }
            }

            Assert.True(success, "Not successful within retry limit.");
        }
    }
}

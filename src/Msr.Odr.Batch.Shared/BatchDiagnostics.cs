// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Msr.Odr.Batch.Shared
{
    public static class BatchDiagnostics
    {
        private const int EnvNameLength = 40;
        private const int EnvValueLength = 60;

        private static readonly string[] HiddenNames =
        {
            "Batch:Key",
            "Batch:StorageKey",
            "Vault:ClientId",
            "Vault:ClientSecret",
        };

        public static readonly string DividerLine = new string('-', EnvNameLength + EnvValueLength + 3);

        private static BatchLogger Log => BatchLogger.Instance;

        public static void DumpEnvironment()
        {
            Log.Add(DividerLine);
            Environment
                .GetEnvironmentVariables()
                .Cast<DictionaryEntry>()
                .Select(de =>
                {
                    var k = de.Key.ToString();
                    var v = IsHidden(k)
                        ? new[] { "..." }
                        : ChopString(de.Value.ToString(), EnvValueLength);
                    return (key: k, value: v);
                })
                .SelectMany(kvp => kvp.value.Select((v, idx) =>
                {
                    var k = (idx == 0 ? kvp.key : string.Empty).PadLeft(EnvNameLength);
                    return $"{k}: {v}";
                }))
                .ToList()
                .ForEach(msg =>
                {
                    Log.Add(msg);
                });
            Log.Add(DividerLine);
        }

        private static IEnumerable<string> ChopString(string text, int width)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                int start = 0;
                while (start < text.Length)
                {
                    int len = Math.Min(text.Length - start, width);
                    yield return text.Substring(start, len);
                    start += len;
                }
            }
        }

        private static bool IsHidden(string name)
        {
            return HiddenNames
                .Any(hidden => string.Compare(name, hidden, StringComparison.InvariantCultureIgnoreCase) == 0);
        }
    }
}

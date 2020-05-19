using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Msr.Odr.Admin.Commands
{
    public static class CommandLineApplicationExtensions
    {
        public static void OnExecuteShowHelp(this CommandLineApplication command)
        {
            command.OnExecute(() =>
            {
                command.ShowHelp();
                return 0;
            });
        }

        public static void SetDefaultHelp(this CommandLineApplication command)
        {
            command.HelpOption("-? | -h | --help");
        }

        public static string LoadValue(this IDictionary<string, string> values, string key)
        {
            if (values == null)
            {
                return null;
            }

            var v = values.TryGetValue(key, out var value) ? value : null;
            return string.IsNullOrWhiteSpace(v) ? null : v;
        }

        public static bool HasAllRequiredParameters(this CommandLineApplication command, IEnumerable<string> required)
        {
            if (required.All((s) => !string.IsNullOrWhiteSpace(s)))
            {
                return true;
            }

            command.ShowHelp();
            return false;
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> list, Func<T, Task> func)
        {
            foreach (var value in list)
            {
                await func(value);
            }
        }
    }
}

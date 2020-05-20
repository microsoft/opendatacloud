using System;
using System.Collections.Generic;
using System.Text;

namespace DatasetUtil.Components
{
    public class CommandArgs
    {
        public static CommandArgs Instance { get; private set; }

        public static void Initialize(string[] args)
        {
            Instance = new CommandArgs(args);

        }

        public string[] Arguments { get; }

        private CommandArgs(string[] args)
        {
            Arguments = args;
        }
    }
}

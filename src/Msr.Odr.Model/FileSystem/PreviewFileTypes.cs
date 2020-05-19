using System;
using System.Collections.Generic;
using System.Text;

namespace Msr.Odr.Model.FileSystem
{
    public static class PreviewFileTypes
    {
        static PreviewFileTypes()
        {
            List = new[]
            {
                "txt",
                "tsv",
                "csv",
                "py",
                "m",
            };
        }

        public static IReadOnlyCollection<string> List { get; }
    }
}

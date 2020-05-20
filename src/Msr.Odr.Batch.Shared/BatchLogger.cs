using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Msr.Odr.Batch.Shared
{
    public class BatchLogger
    {
        public static readonly BatchLogger Instance = new BatchLogger();

        public BatchLogger Add()
        {
            Console.WriteLine();
            return this;
        }

        public BatchLogger Add(string message)
        {
            Console.WriteLine(message);
            return this;
        }

        public BatchLogger BatchError(Exception ex)
        {
            WriteError(Console.Out, ex);
            return this;
        }

        public BatchLogger InitializationError(Exception ex)
        {
            WriteError(Console.Error, ex);
            return this;
        }

        private void WriteError(TextWriter output, Exception ex)
        {
            foreach (var x in NestedExceptions(ex))
            {
                output.WriteLine($"{x.GetType().FullName} - {x.Message}");
            }
            output.WriteLine(ex.StackTrace);
        }

        private IEnumerable<Exception> NestedExceptions(Exception ex)
        {
            while (ex != null)
            {
                yield return ex;
                ex = ex.InnerException;
            }
        }
    }
}

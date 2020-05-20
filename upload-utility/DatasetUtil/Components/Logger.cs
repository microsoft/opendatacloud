using System;

namespace DatasetUtil.Components
{
    public class Logger
    {
        static Logger()
        {
            Instance = new Logger();
        }

        public static Logger Instance { get; }

        public Logger Add()
        {
            Console.WriteLine(string.Empty);
            return this;
        }

        public Logger Add(string message)
        {
            Console.WriteLine(message);
            return this;
        }

        public Logger Status(string message)
        {
            Console.Write($"\r{message}");
            return this;
        }

        public Logger Error(string message, Exception ex)
        {
            Console.WriteLine(message);
            Error(ex);
            return this;
        }

        public Logger Error(Exception ex)
        {
            Console.WriteLine(ex.Message);
            return this;
        }
    }
}

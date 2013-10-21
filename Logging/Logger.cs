using System;
using System.IO;
using System.Threading;

namespace concord.Logging
{
    internal class Logger : ILogger
    {
        public void Log(string s)
        {
            const string tempErrors = @"c:\temp\blah.txt";
            try
            {
                if (!File.Exists(tempErrors)) return;
                Thread.Sleep(3000);
                using (var sw = new StreamWriter(tempErrors, true))
                {
                    sw.WriteLineAsync(s);
                }
            }
            catch (Exception exception)
            {
                Console.Write(exception);
            }
        }
    }
}
using System;
using System.IO;

namespace concord.Wrappers
{
    public class TextWriterWrapper
    {
        private readonly TextWriter _writer;

        public TextWriterWrapper(TextWriter writer)
        {
            _writer = writer;
        }

        public void WriteLine(string format, params object[] arg)
        {
            Safely(() => _writer.WriteLine(format, arg));
        }

        public void Write(string format, params object[] arg)
        {
            Safely(() => _writer.Write(format, arg));
        }

        public void WriteLine()
        {
            Safely(() => _writer.WriteLine());
        }

        private void Safely(Action action)
        {
            try
            {
                action();
            }
            catch (Exception)
            {
                // eh... it's logging
            }
        }
    }
}
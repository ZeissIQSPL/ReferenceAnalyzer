using System;

namespace ReferenceAnalyzer.Core
{
    public class MessageSink : IMessageSink
    {
        public void Write(string message) => Console.WriteLine(message);
    }
}

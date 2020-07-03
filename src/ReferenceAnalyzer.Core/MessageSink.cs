using System;
using System.Collections.Generic;
using System.Text;

namespace ReferenceAnalyzer.Core
{
    public class MessageSink : IMessageSink
    {
        public void Write(string message)
        {
            Console.WriteLine(message);
        }
    }
}

using System;
using System.Collections.ObjectModel;
using DynamicData;

namespace ReferenceAnalyzer.UI.Services
{
    public class MessageSink : IReadableMessageSink
    {
        private readonly ReadOnlyObservableCollection<string> _lines;
        private SourceList<string> _logs;

        public MessageSink()
        {
            _logs = new SourceList<string>();

            _logs.Connect()
                .Bind(out _lines)
                .Subscribe();
        }

        public void Write(string message)
        {
            _logs.Add(message);
        }

        public ReadOnlyObservableCollection<string> Lines => _lines;
    }
}

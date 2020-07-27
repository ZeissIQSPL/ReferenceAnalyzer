using System.Collections.ObjectModel;
using ReferenceAnalyzer.Core;

namespace ReferenceAnalyzer.UI.Services
{
    public interface IReadableMessageSink : IMessageSink
    {
        ReadOnlyObservableCollection<string> Lines { get; }
    }
}

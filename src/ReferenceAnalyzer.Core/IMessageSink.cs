namespace ReferenceAnalyzer.Core
{
    public interface IMessageSink
    {
        void Write(string message);
    }
}
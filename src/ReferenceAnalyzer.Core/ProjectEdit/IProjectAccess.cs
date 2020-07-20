namespace ReferenceAnalyzer.Core.ProjectEdit
{
    public interface IProjectAccess
    {
        string Read(string path);
        void Write(string path);
    }
}

using System.IO;

namespace ReferenceAnalyzer.Core.ProjectEdit
{
    public class ProjectAccess : IProjectAccess
    {
        public string Read(string path)
        {
            return File.ReadAllText(path);
        }

        public void Write(string path, string content)
        {
            File.WriteAllText(path, content);
        }
    }
}

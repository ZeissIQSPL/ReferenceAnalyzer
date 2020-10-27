using System.Collections.Generic;
using System.IO;

namespace ReferenceAnalyzer.Core.ProjectEdit
{
    public class CachedProjectAccess
    {
        private readonly IProjectAccess _projectAccess;
        private readonly Dictionary<string, string> _cache;

        public CachedProjectAccess(IProjectAccess projectAccess)
        {
            _projectAccess = projectAccess;
            _cache = new Dictionary<string, string>();
        }

        public string Read(string path)
        {
            path = Path.GetFullPath(path);
            if (!_cache.ContainsKey(path))
                _cache[path] = _projectAccess.Read(path);

            return _cache[path];
        }

        public void Write(string path, string content)
        {
            _cache.Remove(path);
        }

        public void Invalidate()
        {
            _cache.Clear();
        }
    }
}

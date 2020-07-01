using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace ReferenceExplorer.Models
{
    public class Project
    {
        public AssemblyIdentity Assembly { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }
        public IEnumerable<Reference> FormalReferences { get; set; }
        public ISet<Reference> ActualReferences { get; set; }
        public IEnumerable<Reference> DiffReferences => FormalReferences.Except(ActualReferences);

        public IEnumerable<string> Types { get; set; }

        public Project()
        {
            FormalReferences = Enumerable.Empty<Reference>();
            ActualReferences = new HashSet<Reference>();
        }
        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
	        if (ReferenceEquals(null, obj))
		        return false;
	        if (ReferenceEquals(this, obj))
		        return true;
	        if (obj.GetType() != this.GetType())
		        return false;
	        return Equals((Project) obj);
        }

        protected bool Equals(Project other)
        {
	        return Name == other.Name;
        }

        public override int GetHashCode()
        {
	        return Name?.GetHashCode() ?? 0;
        }
    }
}
using System;
using Microsoft.CodeAnalysis;

namespace ReferenceExplorer.Models
{
    public class Reference
    {
        public string Project { get; set; }
        public int UsagesInSource { get; set; }

        public override string ToString()
        {
            return $"{Project} - {UsagesInSource}";
        }

        public override bool Equals(object? obj)
        {
	        if (ReferenceEquals(null, obj))
		        return false;
	        if (ReferenceEquals(this, obj))
		        return true;
	        if (obj.GetType() != this.GetType())
		        return false;
	        return Equals((Reference) obj);
        }

        protected bool Equals(Reference other)
        {
	        return Project == other.Project;
        }

        public override int GetHashCode()
        {
	        return (Project != null ? Project.GetHashCode() : 0);
        }
    }
}
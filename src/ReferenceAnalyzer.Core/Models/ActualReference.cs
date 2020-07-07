using System.Collections.Generic;

namespace ReferenceAnalyzer.Core
{
    public class ActualReference
    {
        public ActualReference(string target, IEnumerable<ReferenceOccurrence> occurrences)
        {
            Target = target;
            Occurrences = occurrences;
        }

        public IEnumerable<ReferenceOccurrence> Occurrences { get; }
        public string Target { get; }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ActualReference)obj);
        }

        protected bool Equals(ActualReference other) => other != null && Target == other.Target;

        public override int GetHashCode() => Target != null ? Target.GetHashCode() : 0;

        public static bool operator ==(ActualReference left, ActualReference right) => Equals(left, right);

        public static bool operator !=(ActualReference left, ActualReference right) => !Equals(left, right);
    }
}

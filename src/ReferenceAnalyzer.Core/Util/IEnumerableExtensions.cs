using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReferenceAnalyzer.Core.Util
{
    public static class EnumerableExtensions
    {
        public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
                throw new ArgumentNullException(nameof(enumerable));

            foreach (var el in enumerable)
                yield return await Task.FromResult(el);
        }
    }
}

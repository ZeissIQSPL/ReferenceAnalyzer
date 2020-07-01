using System;
using System.Collections.Generic;
using System.Text;

namespace ReferenceExplorer
{
    public static class IEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> enumerable)
        {
            foreach (var el in enumerable)
            {
                yield return el;
            }
        }
    }
}

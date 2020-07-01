using System.Collections.Generic;
using System.Threading.Tasks;

namespace ReferenceAnalyzer.Core.Util
{
	public static class IEnumerableExtensions
	{
		public static async IAsyncEnumerable<T> ToAsync<T>(this IEnumerable<T> enumerable)
		{
			foreach (var el in enumerable)
			{
				yield return await Task.FromResult(el);
			}
		}
	}
}
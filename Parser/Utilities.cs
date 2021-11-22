using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace Parser {
	public static class Utilities {
		public class SetEqualityComparer<T> : IEqualityComparer<ISet<T>> {
			public static SetEqualityComparer<T> Comparer { get; } = new();

			public bool Equals(ISet<T>? x, ISet<T>? y) {
				if (ReferenceEquals(x, y) || x is null && y is null)
					return true;
				if (x is null || y is null)
					return false;
				return x.SetEquals(y);
			}

			public int GetHashCode(ISet<T> obj) {
				int?[] hashCodes = obj.Select(o => o?.GetHashCode()).ToArray();
				Array.Sort(hashCodes);
				return hashCodes.Aggregate(0, HashCode.Combine);
			}
		}
	}
}
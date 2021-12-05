using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public class EventArgs<T> : EventArgs {
			private EventArgs() { }

			public EventArgs(T value) => Value = value;

			public new static EventArgs<T> Empty { get; } = new();

			public T? Value { get; }

			public static implicit operator EventArgs<T>(T value) => new(value);

			public static implicit operator T?(EventArgs<T> args) => args.Value;
		}

		public static int? ReadInteger(this StringReader reader) {
			int ch = reader.Peek();
			if (ch == -1)
				return null;
			var neg = false;
			if (ch is '+' or '-') {
				neg = ch == '-';
				reader.Read();
			}
			if (!char.IsDigit((char)reader.Peek()))
				throw new InvalidOperationException("Not an integer");
			var value = 0;
			while (true) {
				ch = reader.Peek();
				if (!char.IsDigit((char)ch))
					break;
				value = value * 10 + ch - '0';
				if (value < 0)
					throw new OverflowException();
				reader.Read();
			}
			return neg ? -value : value;
		}
	}
}
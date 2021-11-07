using System;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexeme<T> where T : struct, Enum {
		private readonly StringSegment _segment;

		public T Type { get; init; }

		public int Position => _segment.Offset;

		public int Length => _segment.Length;

		public string Value => _segment.Value;

		public string Code => _segment.Buffer;

		public Lexeme(T type, StringSegment segment) {
			Type = type;
			_segment = segment;
		}

		public override string ToString() {
			string typeName = Enum.GetName(Type)!;
			return $"<{typeName}>{Value}</{typeName}>";
		}
	}
}

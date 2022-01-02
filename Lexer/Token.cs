using System;
using Microsoft.Extensions.Primitives;

namespace Lexer {
	public class Token : IEquatable<Token> {
		private readonly StringSegment _segment;

		public Token(Lexeme lexeme, StringSegment segment) {
			Lexeme = lexeme;
			_segment = segment;
		}

		public Lexeme Lexeme { get; init; }

		public StringSegment Segment => _segment;

		public int Position => _segment.Offset;

		public int Length => _segment.Length;

		public string Value => _segment.Value;

		public string Code => _segment.Buffer;

		public bool Equals(Token? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return _segment.Equals(other._segment) && Lexeme.Equals(other.Lexeme);
		}

		public override string ToString() => $"<{Lexeme} Value=\"{Value.Replace(@"\", @"\\").Replace("\"", "\\\"")}\" />";

		public override bool Equals(object? obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj is StringSegment seg
				? this == seg
				: obj.GetType() == GetType() && Equals((Token)obj);
		}

		public override int GetHashCode() => HashCode.Combine(_segment, Lexeme);

		public static bool operator ==(Token self, StringSegment stringSegment) => self._segment == stringSegment;

		public static bool operator !=(Token self, StringSegment stringSegment) => self._segment != stringSegment;
	}
}
using System;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexeme : IEquatable<Lexeme> {
		private readonly StringSegment _segment;

		public Lexeme(Token token, StringSegment segment) {
			Token = token;
			_segment = segment;
		}

		public Token Token { get; init; }

		public StringSegment Segment => _segment;

		public int Position => _segment.Offset;

		public int Length => _segment.Length;

		public string Value => _segment.Value;

		public string Code => _segment.Buffer;

		public bool Equals(Lexeme? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return _segment.Equals(other._segment) && Token.Equals(other.Token);
		}

		public override string ToString() => $"<{Token}>{Value}</{Token}>";

		public override bool Equals(object? obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj is StringSegment seg
				? this == seg
				: obj.GetType() == GetType() && Equals((Lexeme)obj);
		}

		public override int GetHashCode() => HashCode.Combine(_segment, Token);

		public static bool operator ==(Lexeme self, StringSegment stringSegment) => self._segment == stringSegment;

		public static bool operator !=(Lexeme self, StringSegment stringSegment) => self._segment != stringSegment;
	}
}
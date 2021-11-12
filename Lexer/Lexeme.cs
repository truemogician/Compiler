using System;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexeme {
		private readonly StringSegment _segment;

		public Token Token { get; init; }

		public StringSegment Segment => _segment;

		public int Position => _segment.Offset;

		public int Length => _segment.Length;

		public string Value => _segment.Value;

		public string Code => _segment.Buffer;

		public Lexeme(Token token, StringSegment segment) {
			Token = token;
			_segment = segment;
		}

		public override string ToString() => $"<{Token}>{Value}</{Token}>";
	}
}

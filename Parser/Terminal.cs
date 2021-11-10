using System;
using System.Text.RegularExpressions;
using Lexer;

#nullable enable
namespace Parser {
	public delegate bool TerminalMatcher<TToken>(Lexeme<TToken> lexeme) where TToken : struct, Enum;

	public class Terminal<TToken> : IEquatable<Terminal<TToken>> where TToken : struct, Enum {
		private readonly TToken? _type;

		private readonly TerminalMatcher<TToken>? _matcher;

		private Terminal() { }

		public Terminal(TToken tokenType) : this(tokenType, _ => true) { }

		public Terminal(TToken tokenType, TerminalMatcher<TToken> matcher) {
			_type = tokenType;
			_matcher = matcher;
		}

		public Terminal(TToken tokenType, Regex pattern) : this(tokenType, lexeme => pattern.IsMatch(lexeme.Value)) { }

		public Terminal(TToken tokenType, string pattern) : this(tokenType, new Regex(pattern)) { }

		public static Terminal<TToken> Terminator { get; } = new();

		public TToken TokenType => _type ?? throw new NullReferenceException();

		public bool Match(Lexeme<TToken> lexeme) {
			if (_type is null || _matcher is null)
				throw new InvalidOperationException("Terminator cannot match");
			return lexeme.Type.Equals(TokenType) && _matcher(lexeme);
		}

		public bool Equals(Terminal<TToken>? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Nullable.Equals(_type, other._type) && Equals(_matcher, other._matcher);
		}

		public override int GetHashCode() => HashCode.Combine(_type, _matcher);

		public static implicit operator Terminal<TToken>(TToken tokenType) => new(tokenType);
	}
}
using System;
using System.Text.RegularExpressions;
using Lexer;

#nullable enable
namespace Parser {
	/// <summary>
	///     Determine whether a <paramref name="lexeme" /> belongs to a terminal
	/// </summary>
	public delegate bool TerminalMatcher(Lexeme lexeme);

	public class Terminal : IEquatable<Terminal> {
		private readonly TerminalMatcher? _matcher;

		private readonly Token? _token;

		private Terminal() { }

		public Terminal(Token token) : this(token, _ => true) { }

		public Terminal(Token token, TerminalMatcher matcher) {
			_token = token;
			_matcher = matcher;
		}

		public Terminal(Token token, string pattern) : this(token, lexeme => lexeme == pattern) { }

		public Terminal(Token token, Regex pattern) : this(token, lexeme => pattern.IsMatch(lexeme.Value)) { }

		public static Terminal Terminator { get; } = new();

		public Token Token => _token ?? throw new NullReferenceException();

		public bool Equals(Terminal? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return _token == other._token && Equals(_matcher, other._matcher);
		}

		public bool Match(Lexeme lexeme) {
			if (_token is null || _matcher is null)
				throw new InvalidOperationException("Terminator cannot match");
			return lexeme.Token.Equals(Token) && _matcher(lexeme);
		}

		public override int GetHashCode() => HashCode.Combine(_token, _matcher);

		public static implicit operator Terminal(Token tokenType) => new(tokenType);

		public override bool Equals(object? obj) => Equals(obj as Terminal);
	}
}
using System;
using System.Text.RegularExpressions;
using Lexer;

#nullable enable
namespace Parser {
	/// <summary>
	///     Determine whether a <paramref name="token" /> belongs to a terminal
	/// </summary>
	public delegate bool TerminalMatcher(Token token);

	public class Terminal : IEquatable<Terminal> {
		private readonly string? _pattern;

		private readonly TerminalMatcher? _matcher;

		private readonly Lexeme? _lexeme;

		private Terminal() { }

		public Terminal(Lexeme lexeme) : this(lexeme, _ => true) { }

		public Terminal(Lexeme lexeme, TerminalMatcher matcher) {
			_lexeme = lexeme;
			_matcher = matcher;
		}

		public Terminal(Lexeme lexeme, string pattern) : this(lexeme, token => token == pattern) => _pattern = pattern;

		public Terminal(Lexeme lexeme, Regex pattern) : this(lexeme, token => pattern.IsMatch(token.Value)) => _pattern = pattern.ToString();

		public static Terminal Terminator { get; } = new();

		public Lexeme Lexeme => _lexeme ?? throw new NullReferenceException();

		public bool Equals(Terminal? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return _lexeme == other._lexeme && Equals(_matcher, other._matcher);
		}

		public bool Match(Token token) {
			if (_lexeme is null || _matcher is null)
				throw new InvalidOperationException("Terminator cannot match");
			return token.Lexeme.Equals(Lexeme) && _matcher(token);
		}

		public override bool Equals(object? obj) => Equals(obj as Terminal);

		public override int GetHashCode() => HashCode.Combine(_lexeme, _matcher);

		public override string ToString() => _lexeme is null ? "＃" : _pattern is null ? _lexeme.Name : $"{_lexeme.Name}/{_pattern}/";

		public static implicit operator Terminal(Lexeme lexeme) => new(lexeme);

		public static SentenceForm operator +(Terminal left, Symbol right) => (SentenceForm)left + right;
	}
}
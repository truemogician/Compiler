using System;
using System.Text.RegularExpressions;
using Lexer;

namespace Parser {
	public class Terminal : IEquatable<Terminal> {
		private readonly Lexeme? _lexeme;

		private readonly string? _pattern;

		private readonly Regex? _regex;

		private Terminal() { }

		public Terminal(Lexeme lexeme) => _lexeme = lexeme;

		public Terminal(Lexeme lexeme, string pattern) : this(lexeme) => _pattern = pattern;

		public Terminal(Lexeme lexeme, Regex pattern) : this(lexeme) {
			_pattern = pattern.ToString();
			_regex = new Regex(_pattern, pattern.Options | RegexOptions.Compiled);
		}

		public static Terminal Terminator { get; } = new();

		public Lexeme Lexeme => _lexeme ?? throw new NullReferenceException("Terminator has no lexeme");

		public string? Pattern => _lexeme is not null ? _pattern : throw new NullReferenceException("Terminator has no pattern");

		public bool UseRegex => _regex is not null;

		public bool Equals(Terminal? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return _lexeme == other._lexeme && Equals(_pattern, other._pattern) && UseRegex == other.UseRegex;
		}

		public bool Match(Token token) {
			if (Equals(Terminator))
				throw new InvalidOperationException("Terminator cannot match");
			return token.Lexeme.Equals(Lexeme) && (UseRegex ? _regex!.Match(token.Segment).Success : _pattern == token.Segment);
		}

		public override bool Equals(object? obj) => Equals(obj as Terminal);

		public override int GetHashCode() => HashCode.Combine(_lexeme, _pattern, UseRegex);

		public override string ToString() => Equals(Terminator) ? "＃" : _pattern is null ? _lexeme!.Name : $"{_lexeme!.Name}{(UseRegex ? $"/{_pattern}/" : $"({_pattern})")}";

		public static implicit operator Terminal(Lexeme lexeme) => new(lexeme);

		public static SentenceForm operator +(Terminal left, Symbol right) => (SentenceForm)left + right;
	}
}
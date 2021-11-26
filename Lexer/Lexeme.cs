using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexeme : IEquatable<Lexeme> {
		private readonly int? _maxLength;

		private readonly Regex? _regex;

		private Lexeme(string name, string pattern, int? maxLength) {
			if (name.Contains(Environment.NewLine))
				throw new ArgumentException("Lexeme name cannot contains new line characters", nameof(name));
			Name = name;
			Pattern = pattern;
			_maxLength = maxLength;
		}

		/// <summary>
		///     Create a lexeme that matches a specific character
		/// </summary>
		/// <param name="name">Type of the lexeme</param>
		/// <param name="character">The character to be matched</param>
		public Lexeme(string name, char character) : this(name, new string(character, 1), 1) { }

		/// <inheritdoc cref="Lexeme(string, char)" />
		/// <param name="name">Type of the lexeme specified by a enum, the name of the enum value will be used</param>
		public Lexeme(Enum name, char character) : this(GetEnumName(name), character) { }

		/// <summary>
		///     Create a lexeme that matches an exact string
		/// </summary>
		/// <param name="name">Type of the lexeme</param>
		/// <param name="pattern">The string to be matched</param>
		public Lexeme(string name, string pattern) : this(name, pattern, pattern.Length) { }

		/// <param name="name">Type of the lexeme specified by a enum, the name of the enum value will be used</param>
		/// <inheritdoc cref="Lexeme(string, string)" />
		public Lexeme(Enum name, string pattern) : this(GetEnumName(name), pattern) { }

		//TODO calculate maxLength automatically
		/// <summary>
		///     Create a lexeme that matches a regex pattern
		/// </summary>
		/// <param name="name">Type of the lexeme</param>
		/// <param name="pattern">Pattern of the token. Beginning symbol will be automatically added.</param>
		public Lexeme(string name, Regex pattern) : this(name, $"^(?:{pattern})", null) => _regex = new Regex(Pattern, pattern.Options | RegexOptions.Compiled);

		/// <param name="name">Type of the lexeme specified by a enum, the name of the enum value will be used</param>
		/// <inheritdoc cref="Lexeme(string, Regex, int)" />
		public Lexeme(Enum name, Regex pattern) : this(GetEnumName(name), pattern) { }

		public string Name { get; }

		public string Pattern { get; }

		public bool UseRegex => _regex is not null;

		public bool Equals(Lexeme? other) {
			if (other is null)
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Name == other.Name && UseRegex == other.UseRegex && Pattern == other.Pattern;
		}

		public Token? Match(StringSegment input) {
			if (_regex is null)
				return Pattern == input ? new Token(this, input.Subsegment(0, Pattern.Length)) : null;
			var result = _maxLength is null ? _regex.Match(input) : _regex.Match(input, 0, _maxLength.Value);
			return result.Success ? new Token(this, input.Subsegment(0, result.Length)) : null;
		}

		private static string GetEnumName(Enum value) => Enum.GetName(value.GetType(), value) ?? throw new Exception();

		public override string ToString() => Name;

		public override bool Equals(object? obj) {
			if (obj is null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			return obj.GetType() == GetType() && Equals(obj as Lexeme);
		}

		public override int GetHashCode() => HashCode.Combine(Name, Pattern, UseRegex);
	}
}
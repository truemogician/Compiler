using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using OneOf;

#nullable enable
namespace Lexer {
	using LexemeMatcherResult = OneOf<Token, StringSegment, Match, int>;

	public delegate LexemeMatcherResult? LexemeMatcher(StringSegment code);

	public class Lexeme {
		private readonly LexemeMatcher _matcher;

		/// <summary>
		///     Create a lexeme that matches a specific character
		/// </summary>
		/// <param name="name">Type of the lexeme</param>
		/// <param name="character">The character to be matched</param>
		public Lexeme(string name, char character) {
			Name = name;
			_matcher = code => code[0] == character ? 1 : null;
		}

		/// <inheritdoc cref="Lexeme(string, char)" />
		/// <param name="name">Type of the lexeme specified by a enum, the name of the enum value will be used</param>
		public Lexeme(Enum name, char character) : this(GetEnumName(name), character) { }

		/// <summary>
		///     Create a lexeme that matches an exact string
		/// </summary>
		/// <param name="name">Type of the lexeme</param>
		/// <param name="pattern">The string to be matched</param>
		public Lexeme(string name, string pattern) {
			Name = name;
			_matcher = code => pattern.Length <= code.Buffer.Length - code.Offset - code.Length && code.Subsegment(0, pattern.Length) == pattern ? pattern.Length : null;
		}

		/// <param name="name">Type of the lexeme specified by a enum, the name of the enum value will be used</param>
		/// <inheritdoc cref="Lexeme(string, string)" />
		public Lexeme(Enum name, string pattern) : this(GetEnumName(name), pattern) { }

		//TODO calculate maxLength automatically
		/// <summary>
		///     Create a lexeme that matches a regex pattern
		/// </summary>
		/// <param name="name">Type of the lexeme</param>
		/// <param name="pattern">Pattern of the token. Beginning symbol will be automatically added.</param>
		/// <param name="maxLength"></param>
		public Lexeme(string name, Regex pattern, int maxLength = 0) {
			Name = name;
			pattern = new Regex($"^(?:{pattern})", RegexOptions.Compiled);
			_matcher = code => maxLength == 0 ? pattern.Match(code) : pattern.Match(code, 0, maxLength);
		}

		/// <param name="name">Type of the lexeme specified by a enum, the name of the enum value will be used</param>
		/// <inheritdoc cref="Lexeme(string, Regex, int)" />
		public Lexeme(Enum name, Regex pattern, int maxLength = 0) : this(GetEnumName(name), pattern, maxLength) { }

		/// <summary>
		///     Create a lexeme from a custom match function
		/// </summary>
		/// <param name="name">Type of the lexeme</param>
		/// <param name="matcher">The custom match function</param>
		public Lexeme(string name, LexemeMatcher matcher) {
			Name = name;
			_matcher = matcher;
		}

		/// <param name="name">Type of the lexeme specified by a enum, the name of the enum value will be used</param>
		/// <inheritdoc cref="Lexeme(string, LexemeMatcher)" />
		public Lexeme(Enum name, LexemeMatcher matcher) : this(GetEnumName(name), matcher) { }

		public string Name { get; init; }

		public Token? Match(StringSegment input) {
			var result = _matcher(input);
			return result is null
				? null
				: result.Value.Index switch {
					0 => result.Value.AsT0,
					1 => new Token(this, result.Value.AsT1),
					2 => result.Value.AsT2 is {Success: true} match
						? new Token(this, new StringSegment(input.Buffer, input.Offset + match.Index, match.Length))
						: null,
					3 => new Token(this, input.Subsegment(0, result.Value.AsT3)),
					_ => throw new ArgumentOutOfRangeException()
				};
		}

		private static string GetEnumName(Enum value) => Enum.GetName(value.GetType(), value) ?? throw new Exception();

		public override string ToString() => Name;
	}
}
using System;
using Microsoft.Extensions.Primitives;
using System.Text.RegularExpressions;
using OneOf;

#nullable enable
namespace Lexer {
	public delegate OneOf<Lexeme<T>, StringSegment, Match, int>? LexemeMatcher<T>(StringSegment code) where T : struct, Enum;

	public class Token<T> where T : struct, Enum {
		private readonly LexemeMatcher<T> _matcher;

		public T Type { get; init; }

		/// <summary>
		/// Create a token that matches a specific character
		/// </summary>
		/// <param name="type">Type of the token</param>
		/// <param name="character">The character to be matched</param>
		public Token(T type, char character) {
			Type = type;
			_matcher = code => code[0] == character ? 1 : null;
		}

		/// <summary>
		/// Create a token that matches an exact string
		/// </summary>
		/// <param name="type">Type of the token</param>
		/// <param name="pattern">The string to be matched</param>
		public Token(T type, string pattern) {
			Type = type;
			_matcher = code => pattern.Length <= code.Buffer.Length - code.Offset - code.Length && code.Subsegment(0, pattern.Length) == pattern ? pattern.Length : null;
		}

		//TODO calculate maxLength automatically
		/// <summary>
		/// Create a token that matches a regex pattern
		/// </summary>
		/// <param name="type">Type of the token</param>
		/// <param name="pattern">Pattern of the token. Beginning symbol will be automatically added.</param>
		/// <param name="maxLength"></param>
		public Token(T type, Regex pattern, int maxLength = 0) {
			Type = type;
			pattern = new Regex($"^(?:{pattern})", RegexOptions.Compiled);
			_matcher = code => maxLength == 0 ? pattern.Match(code) : pattern.Match(code, 0, maxLength);
		}

		public Token(T type, LexemeMatcher<T> matcher) {
			Type = type;
			_matcher = matcher;
		}

		public Lexeme<T>? Match(StringSegment input) {
			var result = _matcher(input);
			return result is null
				? null
				: result.Value.Index switch {
					0 => result.Value.AsT0,
					1 => new Lexeme<T>(Type, result.Value.AsT1),
					2 => result.Value.AsT2 is {Success: true} match
						? new Lexeme<T>(Type, new StringSegment(input.Buffer, input.Offset + match.Index, match.Length))
						: null,
					3 => new Lexeme<T>(Type, input.Subsegment(0, result.Value.AsT3)),
					_ => throw new ArgumentOutOfRangeException()
				};
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexer<T> where T : struct, Enum {
		public Lexicon<T> Rules { get; } = new();

		/// <inheritdoc cref="Token{T}(T, string, int)"/>
		public void AddRule(T type, string pattern, int maxLength = 0) => Rules.Add(new Token<T>(type, pattern, maxLength));

		/// <inheritdoc cref="Token{T}(T, LexemeMatcher{T})"/>
		public void AddRule(T type, LexemeMatcher<T> match) => Rules.Add(new Token<T>(type, match));

		public IEnumerable<Lexeme<T>> Tokenize(string code, bool checkAmbiguity = false) {
			var segment = new StringSegment(code);
			while (true) {
				Lexeme<T> lexeme;
				if (checkAmbiguity) {
					var lexemes = Rules.MatchAll(segment).ToArray();
					lexeme = lexemes.Length switch {
						0 => throw new NoMatchException(code, segment.Offset),
						1 => lexemes[0],
						_ => throw new AmbiguityException<T>(code, segment.Offset, lexemes.Select(l => l.Type))
					};
				}
				else
					lexeme = Rules.Match(segment) ?? throw new NoMatchException(code, segment.Offset);
				segment = segment.Subsegment(lexeme.Length);
				yield return lexeme;
				if (segment.Offset == code.Length)
					break;
			}
		}
	}
}
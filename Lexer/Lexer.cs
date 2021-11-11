using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexer<T> where T : struct, Enum {
		public Lexicon<T> Rules { get; } = new();

		/// <inheritdoc cref="Token{T}(T, char)"/>
		public void AddToken(T type, char character) => Rules.Add(new Token<T>(type, character));

		/// <inheritdoc cref="Token{T}(T, string)"/>
		public void AddToken(T type, string pattern) => Rules.Add(new Token<T>(type, pattern));

		/// <inheritdoc cref="Token{T}(T, Regex, int)"/>
		public void AddToken(T type, Regex pattern, int maxLength = 0) => Rules.Add(new Token<T>(type, pattern, maxLength));

		/// <inheritdoc cref="Token{T}(T, LexemeMatcher{T})"/>
		public void AddToken(T type, LexemeMatcher<T> match) => Rules.Add(new Token<T>(type, match));

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Lexeme<T>>? tokens) {
			try {
				tokens = Tokenize(code);
				return true;
			}
			catch (LexerException) {
				tokens = null;
				return false;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryTokenize(string code, out IEnumerable<Lexeme<T>>? tokens) => TryTokenize(code, false, out tokens);
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexer {
		public Lexicon Lexicon { get; } = new();

		public void AddToken(Token token) => Lexicon.Add(token);

		/// <inheritdoc cref="Token(string, char)"/>
		public void AddToken(string name, char character) => Lexicon.Add(new Token(name, character));

		/// <inheritdoc cref="Token(string, string)"/>
		public void AddToken(string name, string pattern) => Lexicon.Add(new Token(name, pattern));

		/// <inheritdoc cref="Token(string, Regex, int)"/>
		public void AddToken(string name, Regex pattern, int maxLength = 0) => Lexicon.Add(new Token(name, pattern, maxLength));

		/// <inheritdoc cref="Token(string, LexemeMatcher)"/>
		public void AddToken(string name, LexemeMatcher match) => Lexicon.Add(new Token(name, match));

		/// <inheritdoc cref="Token(Enum, char)"/>
		public void AddToken(Enum name, char character) => Lexicon.Add(new Token(name, character));

		/// <inheritdoc cref="Token(Enum, string)"/>
		public void AddToken(Enum name, string pattern) => Lexicon.Add(new Token(name, pattern));

		/// <inheritdoc cref="Token(Enum, Regex, int)"/>
		public void AddToken(Enum name, Regex pattern, int maxLength = 0) => Lexicon.Add(new Token(name, pattern, maxLength));

		/// <inheritdoc cref="Token(Enum, LexemeMatcher)"/>
		public void AddToken(Enum name, LexemeMatcher match) => Lexicon.Add(new Token(name, match));

		public IEnumerable<Lexeme> Tokenize(string code, bool checkAmbiguity = false) {
			var segment = new StringSegment(code);
			while (true) {
				Lexeme lexeme;
				if (checkAmbiguity) {
					var lexemes = Lexicon.MatchAll(segment).ToArray();
					lexeme = lexemes.Length switch {
						0 => throw new NoMatchException(code, segment.Offset),
						1 => lexemes[0],
						_ => throw new AmbiguityException(code, segment.Offset, lexemes.Select(l => l.Token))
					};
				}
				else
					lexeme = Lexicon.Match(segment) ?? throw new NoMatchException(code, segment.Offset);
				segment = segment.Subsegment(lexeme.Length);
				yield return lexeme;
				if (segment.Offset == code.Length)
					break;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Lexeme>? tokens) {
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
		public bool TryTokenize(string code, out IEnumerable<Lexeme>? tokens) => TryTokenize(code, false, out tokens);
	}
}
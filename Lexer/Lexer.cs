using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexer : ILexer {
		public Lexer(Lexicon lexicon) => Lexicon = lexicon;

		public Lexicon Lexicon { get; }

		public IEnumerable<Lexeme> Tokenize(string code, bool checkAmbiguity = false) {
			var segment = new StringSegment(code);
			while (true) {
				Lexeme lexeme;
				if (checkAmbiguity) {
					var lexemes = Lexicon.MatchAll(segment).ToArray();
					lexeme = lexemes.Length switch {
						0 => throw new TokenNotMatchedException(code, segment.Offset),
						1 => lexemes[0],
						_ => throw new AmbiguityException(code, segment.Offset, lexemes.Select(l => l.Token))
					};
				}
				else
					lexeme = Lexicon.Match(segment) ?? throw new TokenNotMatchedException(code, segment.Offset);
				segment = segment.Subsegment(lexeme.Length);
				yield return lexeme;
				if (segment.Offset == code.Length)
					break;
			}
		}
	}

	public interface ILexer {
		public Lexicon Lexicon { get; }

		public IEnumerable<Lexeme> Tokenize(string code, bool checkAmbiguity = false);

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
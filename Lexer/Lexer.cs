using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

#nullable enable
namespace Lexer {
	public class Lexer : ILexer {
		public Lexer(Lexicon lexicon) => Lexicon = lexicon;

		public Lexicon Lexicon { get; }

		public IEnumerable<Token> Tokenize(string code, bool checkAmbiguity = false) {
			var segment = new StringSegment(code);
			while (true) {
				Token token;
				if (checkAmbiguity) {
					var tokens = Lexicon.MatchAll(segment).ToArray();
					token = tokens.Length switch {
						0 => throw new LexemeNotMatchedException(code, segment.Offset),
						1 => tokens[0],
						_ => throw new AmbiguityException(code, segment.Offset, tokens.Select(l => l.Lexeme))
					};
				}
				else
					token = Lexicon.Match(segment) ?? throw new LexemeNotMatchedException(code, segment.Offset);
				segment = segment.Subsegment(token.Length);
				yield return token;
				if (segment.Offset == code.Length)
					break;
			}
		}
	}
}
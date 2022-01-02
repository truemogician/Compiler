using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace Lexer {
	public class Lexer : ILexer {
		public Lexer(Lexicon lexicon) => Lexicon = lexicon;

		public Lexicon Lexicon { get; }

		IEnumerable<Token> ILexer.Tokenize(string code) => Tokenize(code);

		public IEnumerable<Token> Tokenize(string code, MatchStrategy strategy = MatchStrategy.Longest) {
			var segment = new StringSegment(code);
			while (true) {
				var token = Lexicon.Match(segment, strategy) ?? throw new LexemeNotMatchedException(code, segment.Offset);
				segment = segment.Subsegment(token.Length);
				yield return token;
				if (segment.Offset == code.Length)
					break;
			}
		}
	}
}
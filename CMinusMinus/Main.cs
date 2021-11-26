using System.Collections.Generic;
using System.Linq;
using Language;
using Lexer;
using Parser;

namespace CMinusMinus {
	using CLRParser = Parser.LR.CLR.Parser;
	using RegexLexer = Lexer.Lexer;

	public class CMinusMinus : Language<RegexLexer, CLRParser, CMinusMinusFactory> {
		public static Keyword[] Keywords => Factory.Keywords;

		protected override RegexLexer CreateLexer(Lexicon lexicon) => new(lexicon);

		protected override CLRParser CreateParser(Grammar grammar) => new(grammar);

		public override IEnumerable<Token> Filter(IEnumerable<Token> tokens)
			=> tokens.Where(
				l => l.Lexeme.Name is not (
					nameof(LexemeType.WhiteSpace) or
					nameof(LexemeType.LineBreak) or
					nameof(LexemeType.LineComment) or
					nameof(LexemeType.BlockComment))
			);
	}

	public partial class CMinusMinusFactory : LanguageFactoryBase { }
}
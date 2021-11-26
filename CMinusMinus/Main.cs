using System.Collections.Generic;
using System.Linq;
using Language;
using Lexer;
using Parser;
using Parser.LR;

#nullable enable
namespace CMinusMinus {
	using CLRParser = Parser.LR.CLR.Parser;
	using RegexLexer = Lexer.Lexer;

	public partial class CMinusMinusFactory : LanguageFactoryBase { }

	public class CMinusMinus : Language<RegexLexer, IParser, CMinusMinusFactory> {
		public CMinusMinus() {
			Lexer = new RegexLexer(Lexicon);
			Parser = new CLRParser(Grammar);
		}

		public CMinusMinus(string compiledTablePath) {
			Lexer = new RegexLexer(Lexicon);
			Parser = CompiledParser.Load(compiledTablePath);
		}

		public static Keyword[] Keywords => Factory.Keywords;

		public override RegexLexer Lexer { get; }

		public override IParser Parser { get; }

		public CLRParser? RawParser => Parser as CLRParser;

		public CompiledParser? CompiledParser => Parser as CompiledParser;

		public override IEnumerable<Token> Filter(IEnumerable<Token> tokens)
			=> tokens.Where(
				l => l.Lexeme.Name is not (
					nameof(LexemeType.WhiteSpace) or
					nameof(LexemeType.LineBreak) or
					nameof(LexemeType.LineComment) or
					nameof(LexemeType.BlockComment))
			);
	}
}
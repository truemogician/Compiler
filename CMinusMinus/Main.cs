using System.Collections.Generic;
using System.Linq;
using Language;
using Lexer;

namespace CMinusMinus {
	using CLRParser = Parser.LR.CLR.Parser;

	public partial class CMinusMinus : Language<Lexer.Lexer, CLRParser> {
		public CMinusMinus() {
			Lexer = new Lexer.Lexer(InitializeLexicon());
			Parser = new CLRParser(InitializeGrammar());
		}

		public override Lexer.Lexer Lexer { get; }

		public override CLRParser Parser { get; }

		public override IEnumerable<Token> Format(IEnumerable<Token> tokens)
			=> tokens.Where(
				l => l.Lexeme.Name is not (
					nameof(TokenType.WhiteSpace) or
					nameof(TokenType.LineBreak) or
					nameof(TokenType.LineComment) or
					nameof(TokenType.BlockComment))
			);
	}
}
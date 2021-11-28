using System;
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
		private bool _useCompiled;

		public CMinusMinus() => RawParser = new CLRParser(Grammar);

		public CMinusMinus(string compiledTablePath) => CompiledParser = CompiledParser.Load(compiledTablePath);

		public static Keyword[] Keywords => Factory.Keywords;

		public override RegexLexer Lexer { get; } = new(Lexicon);

		public override IParser Parser => RawParser is not null && !_useCompiled ? RawParser : CompiledParser!;

		public CLRParser? RawParser { get; }

		public CompiledParser? CompiledParser { get; private set; }

		public void UseCompiledParser() {
			CompiledParser ??= RawParser!.Compile();
			_useCompiled = true;
		}

		public void UseRawParser() {
			if (RawParser is null)
				throw new InvalidOperationException("Table file loaded instance doesn't have a raw parser");
			_useCompiled = false;
		}

		public void InitializeRawParser() {
			if (RawParser is null)
				throw new InvalidOperationException("Table file loaded instance doesn't have a raw parser");
			RawParser.Initialize();
		}

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
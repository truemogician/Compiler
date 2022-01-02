using System;
using System.Collections.Generic;
using System.Linq;
using Language;
using Lexer;
using Parser;

namespace CMinusMinus {
	using RegexLexer = Lexer.Lexer;
	using CLRParser = Parser.LR.CLR.Parser;
	using CompiledCLRParser = Parser.LR.CLR.CompiledParser;
	using GLRParser = Parser.LR.GLR.Parser;
	using CompiledGLRParser = Parser.LR.GLR.CompiledParser;

	public partial class CMinusMinusFactory : LanguageFactoryBase { }

	public class CMinusMinus : Language<RegexLexer, IParser, CMinusMinusFactory> {
		private bool _useCompiled;

		private ParserAlgorithm? _algorithm;

		public CMinusMinus() { }

		public CMinusMinus(ParserAlgorithm parserAlgorithm, bool initialize = false) {
			_algorithm = parserAlgorithm;
			CreateParser(parserAlgorithm);
			if (initialize)
				InitializeParser(parserAlgorithm);
		}

		public CMinusMinus(ParserAlgorithm parserAlgorithm, string compiledTablePath) {
			_algorithm = parserAlgorithm;
			_useCompiled = true;
			LoadCompiledParser(parserAlgorithm, compiledTablePath);
		}

		public static Keyword[] Keywords => Factory.Keywords;

		public override RegexLexer Lexer { get; } = new(Lexicon);

		public override IParser Parser
			=> _algorithm is null
				? throw new InvalidOperationException("A specific parser needs to be selected before parsing")
				: CurrentParser ?? throw new NullReferenceException("Selected parser not created");

		public CLRParser? CLRParser { get; private set; }

		public GLRParser? GLRParser { get; private set; }

		public CompiledCLRParser? CompiledCLRParser { get; private set; }

		public CompiledGLRParser? CompiledGLRParser { get; private set; }

		private IParser? CurrentParser
			=> _algorithm switch {
				null                          => null,
				ParserAlgorithm.CanonicalLR   => _useCompiled ? CompiledCLRParser : CLRParser,
				ParserAlgorithm.GeneralizedLR => _useCompiled ? CompiledGLRParser : GLRParser
			};

		public void SelectParser(ParserAlgorithm parserAlgorithm, bool compiled) {
			_algorithm = parserAlgorithm;
			_useCompiled = compiled;
			if (CurrentParser is null)
				throw new InvalidOperationException("Target parser hasn't been created");
		}

		/// <summary>
		///     Create the parser if not created. Note that raw parser needs initialization before using.
		/// </summary>
		public void CreateParser(ParserAlgorithm parserAlgorithm) {
			switch (parserAlgorithm) {
				case ParserAlgorithm.CanonicalLR:
					CLRParser ??= new CLRParser(Grammar);
					break;
				case ParserAlgorithm.GeneralizedLR:
					GLRParser ??= new GLRParser(Grammar);
					break;
			}
		}

		public void InitializeParser(ParserAlgorithm parserAlgorithm, bool checkConflicts = true) {
			switch (parserAlgorithm) {
				case ParserAlgorithm.CanonicalLR:
					if (CLRParser is null)
						throw new InvalidOperationException("Canonical parser not created");
					CLRParser.Initialize(checkConflicts);
					break;
				case ParserAlgorithm.GeneralizedLR:
					if (GLRParser is null)
						throw new InvalidOperationException("Generalized parser not created");
					GLRParser.Initialize(checkConflicts);
					break;
			}
		}

		public void CompileParser(ParserAlgorithm parserAlgorithm) {
			InitializeParser(parserAlgorithm);
			switch (parserAlgorithm) {
				case ParserAlgorithm.CanonicalLR:
					CompiledCLRParser = CLRParser!.Compile();
					break;
				case ParserAlgorithm.GeneralizedLR:
					CompiledGLRParser = GLRParser!.Compile();
					break;
			}
		}

		public void LoadCompiledParser(ParserAlgorithm parserAlgorithm, string compiledTablePath) {
			switch (parserAlgorithm) {
				case ParserAlgorithm.CanonicalLR:
					CompiledCLRParser = CompiledCLRParser.Load(compiledTablePath);
					break;
				case ParserAlgorithm.GeneralizedLR:
					CompiledGLRParser = CompiledGLRParser.Load(compiledTablePath);
					break;
			}
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

	public enum ParserAlgorithm : byte {
		CanonicalLR,

		GeneralizedLR
	}
}
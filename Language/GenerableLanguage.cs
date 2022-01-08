using System;
using System.Collections.Generic;
using System.Linq;
using Analyzer;
using IntermediateCodeGenerator;
using Lexer;
using Parser;

namespace Language {
	public abstract class GenerableLanguage<TLexer, TParser, TGenerator, TFactory> : AnalyzableLanguage<TLexer, TParser, TFactory>, IGenerableLanguage where TLexer : ILexer where TParser : IParser where TGenerator : IIntermediateCodeGenerator where TFactory : GenerableLanguageFactoryBase<TGenerator>, new() {
		IIntermediateCodeGenerator IGenerableLanguage.Generator => Generator;

		public static TGenerator Generator => Factory.Generator;

		public IEnumerable<IIntermediateCode> Generate(string code) => ((IGenerableLanguage)this).Generate(code);
	}

	public interface IGenerableLanguage : IAnalyzableLanguage {
		public IIntermediateCodeGenerator Generator { get; }

		public IEnumerable<IIntermediateCode> Generate(string code) {
			var results = Analyze(code, out var errors);
			var es = errors.ToArray();
			if (es.Any(e => e.Type.Level == ErrorLevel.Error))
				throw new SemanticErrorException { Errors = es };
			return Generator.Generate(results);
		}
	}

	public abstract class GenerableLanguageFactoryBase<TGenerator> : AnalyzableLanguageFactoryBase where TGenerator : IIntermediateCodeGenerator {
		private readonly Lazy<TGenerator> _generator;

		protected GenerableLanguageFactoryBase() => _generator = new Lazy<TGenerator>(CreateGenerator);

		public TGenerator Generator => _generator.Value;

		public abstract TGenerator CreateGenerator();
	}
}
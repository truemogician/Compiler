using System;
using System.Collections.Generic;
using Analyzer;
using Lexer;
using Parser;

namespace Language {
	public abstract class AnalyzableLanguage<TLexer, TParser, TFactory> : Language<TLexer, TParser, TFactory>, IAnalyzableLanguage where TLexer : ILexer where TParser : IParser where TFactory : AnalyzableLanguageFactoryBase, new() {
		AnalyzerCollection IAnalyzableLanguage.Analyzers => Analyzers;

		public static AnalyzerCollection Analyzers => Factory.Analyzers;

		public IEnumerable<SemanticError> Analyze(string code) {
			var tree = Parse(code);
			tree.Clean();
			return Analyzers.Analyze(tree);
		}
	}

	public interface IAnalyzableLanguage : ILanguage {
		public AnalyzerCollection Analyzers { get; }

		public IEnumerable<SemanticError> Analyze(string code) {
			var tree = Parse(code);
			tree.Clean();
			return Analyzers.Analyze(tree);
		}

		protected IReadOnlyList<AnalyzeResult> Analyze(string code, out IEnumerable<SemanticError> errors) {
			var tree = Parse(code);
			tree.Clean();
			errors = Analyzers.Analyze(tree, out var results);
			return results;
		}
	}

	public abstract class AnalyzableLanguageFactoryBase : LanguageFactoryBase {
		private readonly Lazy<AnalyzerCollection> _analyzers;

		protected AnalyzableLanguageFactoryBase() => _analyzers = new Lazy<AnalyzerCollection>(CreateAnalyzers);

		public AnalyzerCollection Analyzers => _analyzers.Value;

		public abstract AnalyzerCollection CreateAnalyzers();
	}
}
using System;
using Analyzer;
using Lexer;
using Parser;

namespace Language {
	public abstract class LanguageFactoryBase {
		private readonly Lazy<Grammar> _grammar;

		private readonly Lazy<Lexicon> _lexicon;

		private readonly Lazy<AnalyzerCollection> _analyzers;

		protected LanguageFactoryBase() {
			_grammar = new Lazy<Grammar>(CreateGrammar);
			_lexicon = new Lazy<Lexicon>(CreateLexicon);
			_analyzers = new Lazy<AnalyzerCollection>(CreateAnalyzers);
		}

		public Lexicon Lexicon => _lexicon.Value;

		public Grammar Grammar => _grammar.Value;

		public AnalyzerCollection Analyzers => _analyzers.Value;

		public abstract Lexicon CreateLexicon();

		public abstract Grammar CreateGrammar();

		public abstract AnalyzerCollection CreateAnalyzers();
	}
}
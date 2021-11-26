using Lexer;
using Parser;

#nullable enable
namespace Language {
	public abstract class LanguageFactoryBase {
		private Grammar? _grammar;

		private Lexicon? _lexicon;

		public Lexicon Lexicon => _lexicon ??= CreateLexicon();

		public Grammar Grammar => _grammar ??= CreateGrammar();

		public abstract Lexicon CreateLexicon();

		public abstract Grammar CreateGrammar();
	}
}
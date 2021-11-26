using Lexer;
using Parser;

namespace Language.Test {
	public class Sample : Language<Lexer.Lexer, Parser.LR.CLR.Parser, SampleFactory> {
		protected override Lexer.Lexer CreateLexer(Lexicon lexicon) => new(lexicon);

		protected override Parser.LR.CLR.Parser CreateParser(Grammar grammar) => new(grammar);
	}

	public class SampleFactory : LanguageFactoryBase {
		public override Lexicon CreateLexicon() => new() {{"a", 'a'}, {"b", 'b'}};

		public override Grammar CreateGrammar() {
			var s = new Nonterminal("S");
			var b = new Nonterminal("B");
			return new Grammar(s) {{s, b + b}, {b, Lexicon["b"]}, {b, (SentenceForm)Lexicon["a"] + b}};
		}
	}
}
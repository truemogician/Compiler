using Lexer;
using Parser;

namespace Language.Test {
	public class Sample : Language<Lexer.Lexer, Parser.LR.CLR.Parser, SampleFactory> {
		public override Lexer.Lexer Lexer { get; } = new(Lexicon);

		public override Parser.LR.CLR.Parser Parser { get; } = new(Grammar);
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
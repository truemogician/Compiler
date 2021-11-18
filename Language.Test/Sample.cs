using Lexer;
using Parser;

namespace Language.Test {
	public class Sample : Language<Lexer.Lexer, Parser.LR.CLR.Parser> {
		public Sample() {
			var lexicon = new Lexicon {{"a", 'a'}, {"b", 'b'}};
			Lexer = new Lexer.Lexer(lexicon);
			var s = new Nonterminal("S");
			var b = new Nonterminal("B");
			var grammar = new Grammar(s) {{s, b + b}, {b, lexicon["b"]}, {b, (SentenceForm)lexicon["a"] + b}};
			Parser = new Parser.LR.CLR.Parser(grammar);
		}

		public override Lexer.Lexer Lexer { get; }

		public override Parser.LR.CLR.Parser Parser { get; }
	}
}
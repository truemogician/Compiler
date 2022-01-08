using Lexer;
using Parser;

namespace Language.Test {
	using CLRParser = Parser.LR.CLR.Parser;

	public class Sample : Language<Lexer.Lexer, CLRParser, SampleFactory> {
		private CLRParser? _parser;

		public override Lexer.Lexer Lexer { get; } = new(Lexicon);

		public override CLRParser Parser {
			get {
				if (_parser is null) {
					_parser = new CLRParser(Grammar);
					_parser.Initialize();
				}
				return _parser;
			}
		}
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
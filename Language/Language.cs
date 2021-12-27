using System.Collections.Generic;
using System.Linq;
using Analyzer;
using Lexer;
using Parser;

#nullable enable
namespace Language {
	public abstract class Language<TLexer, TParser, TFactory> : ILanguage where TLexer : ILexer where TParser : IParser where TFactory : LanguageFactoryBase, new() {
		public static Lexicon Lexicon => Factory.Lexicon;

		public static Grammar Grammar => Factory.Grammar;

		public abstract TLexer Lexer { get; }

		public abstract TParser Parser { get; }

		public ICollection<IAnalyzer> Analyzers { get; } = new List<IAnalyzer>();

		protected static TFactory Factory { get; } = new();

		ILexer ILanguage.Lexer => Lexer;

		IParser ILanguage.Parser => Parser;

		public IEnumerable<Token> Tokenize(string code) => Lexer.Tokenize(code);

		public bool TryTokenize(string code, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public SyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out SyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);
	}

	public interface ILanguage {
		public ILexer Lexer { get; }

		public IParser Parser { get; }

		public ICollection<IAnalyzer> Analyzers { get; }

		public IEnumerable<Token> Tokenize(string code) => Lexer.Tokenize(code);

		public bool TryTokenize(string code, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public SyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out SyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);

		public IEnumerable<SemanticError> Analyze(string code) {
			var tree = Parse(code);
			tree.Clean();
			return Analyzers.SelectMany(analyzer => analyzer.Analyze(tree));
		}
	}
}
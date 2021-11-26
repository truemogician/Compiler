using System.Collections.Generic;
using Lexer;
using Parser;

#nullable enable
namespace Language {
	public abstract class Language<TLexer, TParser, TFactory> : ILanguage where TLexer : ILexer where TParser : IParser where TFactory : LanguageFactoryBase, new() {
		public static Lexicon Lexicon => Factory.Lexicon;

		public static Grammar Grammar => Factory.Grammar;

		public abstract TLexer Lexer { get; }

		public abstract TParser Parser { get; }

		protected static TFactory Factory { get; } = new();

		ILexer ILanguage.Lexer => Lexer;

		IParser ILanguage.Parser => Parser;

		public IEnumerable<Token> Tokenize(string code, bool checkAmbiguity = false) => Lexer.Tokenize(code, checkAmbiguity);

		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, checkAmbiguity, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public AbstractSyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out AbstractSyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);
	}

	public interface ILanguage {
		public ILexer Lexer { get; }

		public IParser Parser { get; }

		public IEnumerable<Token> Tokenize(string code, bool checkAmbiguity = false) => Lexer.Tokenize(code, checkAmbiguity);

		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, checkAmbiguity, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public AbstractSyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out AbstractSyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);
	}
}
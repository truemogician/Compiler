using System;
using System.Collections.Generic;
using Lexer;
using Parser;

namespace Language {
	public abstract class Language<TLexer, TParser, TFactory> : ILanguage where TLexer : ILexer where TParser : IParser where TFactory : LanguageFactoryBase, new() {
		ILexer ILanguage.Lexer => Lexer;

		IParser ILanguage.Parser => Parser;

		public IEnumerable<Token> Tokenize(string code) => Lexer.Tokenize(code);

		public bool TryTokenize(string code, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public SyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out SyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);

		public static Lexicon Lexicon => Factory.Lexicon;

		public static Grammar Grammar => Factory.Grammar;

		public abstract TLexer Lexer { get; }

		public abstract TParser Parser { get; }

		protected static TFactory Factory { get; } = new();
	}

	public interface ILanguage {
		public ILexer Lexer { get; }

		public IParser Parser { get; }

		public IEnumerable<Token> Tokenize(string code) => Lexer.Tokenize(code);

		public bool TryTokenize(string code, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public SyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out SyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);
	}

	public abstract class LanguageFactoryBase {
		private readonly Lazy<Grammar> _grammar;

		private readonly Lazy<Lexicon> _lexicon;

		protected LanguageFactoryBase() {
			_grammar = new Lazy<Grammar>(CreateGrammar);
			_lexicon = new Lazy<Lexicon>(CreateLexicon);
		}

		public Lexicon Lexicon => _lexicon.Value;

		public Grammar Grammar => _grammar.Value;

		public abstract Lexicon CreateLexicon();

		public abstract Grammar CreateGrammar();
	}
}
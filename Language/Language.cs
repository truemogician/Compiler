using System.Collections.Generic;
using Lexer;
using Parser;

namespace Language {
	public abstract class Language<TLexer, TParser> : ILanguage where TLexer : ILexer where TParser : IParser {
		public abstract TLexer Lexer { get; }

		public abstract TParser Parser { get; }

		ILexer ILanguage.Lexer => Lexer;

		IParser ILanguage.Parser => Parser;

		public Lexicon Lexicon => Lexer.Lexicon;

		public Grammar Grammar => Parser.Grammar;

		public IEnumerable<Lexeme> Tokenize(string code, bool checkAmbiguity = false) => Lexer.Tokenize(code, checkAmbiguity);

		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Lexeme> lexeme) => Lexer.TryTokenize(code, checkAmbiguity, out lexeme);

		public virtual IEnumerable<Lexeme> Format(IEnumerable<Lexeme> lexemes) => lexemes;

		public AbstractSyntaxTree Parse(string code) => Parser.Parse(Format(Tokenize(code)));

		public bool TryParse(string code, out AbstractSyntaxTree ast) => Parser.TryParse(Format(Tokenize(code)), out ast);
	}

	public interface ILanguage {
		public ILexer Lexer { get; }

		public IParser Parser { get; }

		public IEnumerable<Lexeme> Tokenize(string code, bool checkAmbiguity = false) => Lexer.Tokenize(code, checkAmbiguity);

		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Lexeme> lexeme) => Lexer.TryTokenize(code, checkAmbiguity, out lexeme);

		public virtual IEnumerable<Lexeme> Format(IEnumerable<Lexeme> lexemes) => lexemes;

		public AbstractSyntaxTree Parse(string code) => Parser.Parse(Format(Tokenize(code)));

		public bool TryParse(string code, out AbstractSyntaxTree ast) => Parser.TryParse(Format(Tokenize(code)), out ast);
	}
}

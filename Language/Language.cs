using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Lexer;
using Parser;

#nullable enable
namespace Language {
	public abstract class Language<TLexer, TParser, TFactory> : ILanguage where TLexer : ILexer where TParser : IParser where TFactory : LanguageFactoryBase, new() {
		[SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
		protected Language() {
			Lexer = CreateLexer(Lexicon);
			Parser = CreateParser(Grammar);
		}

		public static Lexicon Lexicon => Factory.Lexicon;

		public static Grammar Grammar => Factory.Grammar;

		public TLexer Lexer { get; }

		public TParser Parser { get; }

		protected static TFactory Factory { get; } = new();

		Lexicon ILanguage.Lexicon => Lexicon;

		Grammar ILanguage.Grammar => Grammar;

		ILexer ILanguage.Lexer => Lexer;

		IParser ILanguage.Parser => Parser;

		public IEnumerable<Token> Tokenize(string code, bool checkAmbiguity = false) => Lexer.Tokenize(code, checkAmbiguity);

		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, checkAmbiguity, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public AbstractSyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out AbstractSyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);

		/// <summary>
		///     Create the lexer using <paramref name="lexicon" />. Note that this method is called in constructor, thus all
		///     properties are not initialized and should not be used.
		/// </summary>
		protected abstract TLexer CreateLexer(Lexicon lexicon);

		/// <summary>
		///     Create the parser using <paramref name="grammar" />. Not that this method is called in constructor after
		///     CreateLexer, thus Lexicon and Lexer are initialized and could be used.
		/// </summary>
		protected abstract TParser CreateParser(Grammar grammar);
	}

	public interface ILanguage {
		public Lexicon Lexicon { get; }

		public Grammar Grammar { get; }

		public ILexer Lexer { get; }

		public IParser Parser { get; }

		public IEnumerable<Token> Tokenize(string code, bool checkAmbiguity = false) => Lexer.Tokenize(code, checkAmbiguity);

		public bool TryTokenize(string code, bool checkAmbiguity, out IEnumerable<Token>? tokens) => Lexer.TryTokenize(code, checkAmbiguity, out tokens);

		public virtual IEnumerable<Token> Filter(IEnumerable<Token> tokens) => tokens;

		public AbstractSyntaxTree Parse(string code) => Parser.Parse(Filter(Tokenize(code)));

		public bool TryParse(string code, out AbstractSyntaxTree? ast) => Parser.TryParse(Filter(Tokenize(code)), out ast);
	}
}
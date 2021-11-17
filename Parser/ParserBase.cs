using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Lexer;

#nullable enable
namespace Parser {
	public abstract class ParserBase : IParser {
		// ReSharper disable once VirtualMemberCallInConstructor
		protected ParserBase(Grammar grammar) {
			Grammar = grammar;
			Initialize(grammar);
		}

		public Grammar Grammar { get; }

		public abstract AbstractSyntaxTree Parse(IEnumerable<Lexeme> lexemes);

		protected abstract void Initialize(Grammar grammar);
	}

	public interface IParser {
		public Grammar Grammar { get; }

		public AbstractSyntaxTree Parse(IEnumerable<Lexeme> lexemes);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryParse(IEnumerable<Lexeme> lexemes, out AbstractSyntaxTree? ast) {
			try {
				ast = Parse(lexemes);
				return true;
			}
			catch {
				ast = null;
				return false;
			}
		}
	}
}
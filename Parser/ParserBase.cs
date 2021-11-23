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

		public abstract AbstractSyntaxTree Parse(IEnumerable<Token> tokens);

		protected abstract void Initialize(Grammar grammar);
	}

	public interface IParser {
		public Grammar Grammar { get; }

		public AbstractSyntaxTree Parse(IEnumerable<Token> tokens);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryParse(IEnumerable<Token> tokens, out AbstractSyntaxTree? ast) {
			try {
				ast = Parse(tokens);
				return true;
			}
			catch {
				ast = null;
				return false;
			}
		}
	}
}
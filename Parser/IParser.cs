using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Lexer;

namespace Parser {
	public interface IParser {
		public SyntaxTree Parse(IEnumerable<Token> tokens);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryParse(IEnumerable<Token> tokens, out SyntaxTree? ast) {
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